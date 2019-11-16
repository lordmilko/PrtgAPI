using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.Model;

namespace PrtgAPI.CodeGenerator
{
    class TemplateEvaluator
    {
        private IMethodImpl methodImpl;
        private Template template;
        private ReadOnlyCollection<Template> templates;

        /// <summary>
        /// Merges any method specific methods and regions and resolves any templated method/region redirections.
        /// </summary>
        /// <param name="method">The method that is instantiating this template</param>
        /// <param name="template">The template that is being instantiated.</param>
        /// <param name="templates">A list of all known templates.</param>
        /// <returns></returns>
        public TemplateEvaluator(IMethodImpl method, Template template,
            ReadOnlyCollection<Template> templates)
        {
            methodImpl = method;
            this.template = template;
            this.templates = templates;
        }

        public Template ResolveAll()
        {
            //Resolve any methods defined in regions in this template which simply point to methods in regions in other templates
            var regions = ResolveRegionsContainingTemplatedMethods(template.Regions);

            //Insert any type specific regions (and methods therein) defined in the method that implements this template
            regions = InsertTypeSpecificRegionsMethodsAndOverrides(regions);

            //Resolve any methods defined in this template which simply point to methods in other templates
            var methods = ResolveTemplatedMethods(template.MethodDefs);

            //Insert any type specific methods defined the method that implements this template
            methods = InsertTypeSpecificMethodsAndOverrides(methods);

            if (regions != template.Regions || methods != template.MethodDefs)
                return new Template(template, regions, methods);

            return template;
        }

        #region Region+Method Overrides/Insertions

        private ReadOnlyCollection<RegionDef> InsertTypeSpecificRegionsMethodsAndOverrides(ReadOnlyCollection<RegionDef> originalRegions)
        {
            return InsertTypeSpecificObjectsAndOverrides(originalRegions, methodImpl.Regions, GetRegionOverride);
        }

        private RegionDef GetRegionOverride(RegionDef originalRegion, RegionDef regionOverride)
        {
            var modified = false;

            var newList = new List<IElementDef>();

            foreach (var element in originalRegion.Elements)
            {
                if (element is RegionDef)
                {
                    var replacementRegion = regionOverride.Elements.OfType<RegionDef>().FirstOrDefault(r => r.Name == ((RegionDef) element).Name);

                    if (replacementRegion == null)
                        newList.Add(element);
                    else
                    {
                        var newRegion = GetRegionOverride((RegionDef) element, replacementRegion);

                        modified = true;
                        newList.Add(newRegion);
                    }
                }
                else if (element is MethodDef)
                {
                    var replacementMethod = regionOverride.Elements.OfType<MethodDef>().FirstOrDefault(m => m.Name == ((MethodDef) element).Name);

                    if (replacementMethod == null)
                        newList.Add(element);
                    else
                    {
                        var newMethod = MethodDef.Merge((MethodDef) element, replacementMethod);

                        modified = true;
                        newList.Add(newMethod);
                    }

                    
                }
            }

            if (modified)
                return new RegionDef(originalRegion, false, newList.ToReadOnlyList());

            return originalRegion;
        }

        private MethodDef GetMethodOverride(MethodDef originalMethod, MethodDef methodOverride)
        {
            return MethodDef.Merge(originalMethod, methodOverride);
        }

        private ReadOnlyCollection<MethodDef> InsertTypeSpecificMethodsAndOverrides(ReadOnlyCollection<MethodDef> originalMethods)
        {
            return InsertTypeSpecificObjectsAndOverrides(originalMethods, methodImpl.MethodDefs, GetMethodOverride);
        }

        private ReadOnlyCollection<T> InsertTypeSpecificObjectsAndOverrides<T>(ReadOnlyCollection<T> originalObjects, ReadOnlyCollection<T> overrideObjects, Func<T, T, T> getOverride)
            where T : IInsertableDefinition
        {
            var list = new List<T>();
            var modified = false;

            foreach (var obj in originalObjects)
            {
                var objOverride = GetNextOverride(overrideObjects, obj);

                if (objOverride != null)
                {
                    //We're doing an override!
                    var newObj = getOverride(obj, objOverride);

                    modified = true;
                    list.Add(newObj);
                }
                else
                    list.Add(obj);

                var last = list.Last();

                var region = last as RegionDef;

                if (region != null)
                {
                    if (region.CancellationToken)
                    {
                        modified = true;
                        region.HasTokenRegion = true;
                        list.Add((T)(object)new RegionDef(region, true));
                    }
                }

                var nextObj = overrideObjects.SingleOrDefault(m => m.After == obj.Name);

                if (nextObj != null)
                {
                    //This object needs to go next!
                    modified = true;
                    list.Add(nextObj);
                }
            }

            var trailingObjects = overrideObjects.Where(m => m.After == "*").ToList();

            if (trailingObjects.Count > 0)
            {
                list.AddRange(trailingObjects);
                modified = true;
            }

            if (modified)
                return list.ToReadOnlyList();

            return originalObjects;
        }

        private T GetNextOverride<T>(ReadOnlyCollection<T> overrideObjects, T original) where T : IInsertableDefinition
        {
            return overrideObjects.SingleOrDefault(r =>
            {
                var nameMatch = r.Name == original.Name;

                if (nameMatch && original is MethodDef)
                {
                    if (((MethodDef)(object)r).Overload == ((MethodDef)(object)original).Overload)
                        return true;

                    return false;
                }
                else
                    return nameMatch;
            });
        }
        
        #endregion
        #region Template Redirection

        private ReadOnlyCollection<RegionDef> ResolveRegionsContainingTemplatedMethods(ReadOnlyCollection<RegionDef> pointerRegions)
        {
            var list = new List<RegionDef>();

            bool modified = false;

            foreach (var region in pointerRegions)
            {
                var originalRegions = region.Elements.OfType<RegionDef>().ToReadOnlyList();
                var originalMethods = region.Elements.OfType<MethodDef>().ToReadOnlyList();

                var newRegions = ResolveRegionsContainingTemplatedMethods(originalRegions);

                var newMethods = ResolveTemplatedMethods(originalMethods);

                if (newRegions != originalRegions || newMethods != originalMethods)
                {
                    modified = true;

                    var elements = new List<IElementDef>();
                    elements.AddRange(newRegions);
                    elements.AddRange(newMethods);

                    list.Add(new RegionDef(region, false, elements.ToReadOnlyList()));
                }
                else
                    list.Add(region);
            }

            if (modified)
                return list.ToReadOnlyList();

            return pointerRegions;
        }

        /// <summary>
        /// Resolves a <see cref="MethodDef"/> defined in one <see cref="Template"/> or template <see cref="RegionDef"/> to a <see cref="MethodDef"/> defined somewhere in the hierarchy of another template.
        /// </summary>
        /// <returns></returns>
        private ReadOnlyCollection<MethodDef> ResolveTemplatedMethods(ReadOnlyCollection<MethodDef> pointerDefs)
        {
            var list = new List<MethodDef>();

            bool modified = false;

            foreach (var ptr in pointerDefs)
            {
                if (ptr.Template != null)
                {
                    var targetMethod = ResolveTemplatedMethod(ptr);

                    var mergedMethod = MethodDef.Merge(targetMethod, ptr);

                    modified = true;
                    list.Add(mergedMethod);
                }
                else
                    list.Add(ptr);
            }

            if (modified)
                return list.ToReadOnlyList();

            //Seems our pointers were real definitions after all!
            return pointerDefs;
        }

        private MethodDef ResolveTemplatedMethod(MethodDef pointerMethod)
        {
            var candidates = GetTemplatedMethodResolutionCandidates(pointerMethod).ToList();

            var match = candidates.SingleOrDefault(c =>
            {
                //Does it have the same name?
                if (c.Name != pointerMethod.Name)
                    return false;

                //Does it have the same number of parameters?
                var parameterNames = c.Parameters.Where(p => !p.StreamOnly && !p.TokenOnly).Select(p => p.Name).ToList();

                //Same parameters (none)?
                if (parameterNames.Count == 0)
                {
                    if (pointerMethod.TemplateParametersRaw == "none")
                        return true;

                    return false;
                }

                //Same parameters (count)?
                if (parameterNames.Count != pointerMethod.TemplateParameters.Length)
                    return false;

                //Same parameters names?
                for (var i = 0; i < parameterNames.Count; i++)
                {
                    if (i >= pointerMethod.TemplateParameters.Length || pointerMethod.TemplateParameters[i] != parameterNames[i])
                        return false;
                }

                return true;
            });

            if (match == null)
                throw new NotImplementedException($"Couldn't find the method pointer '{pointerMethod}' corresponds to");

            return match;
        }

        private IEnumerable<MethodDef> GetTemplatedMethodResolutionCandidates(MethodDef pointerMethod)
        {
            //Get the template containing the method we're after
            var targetTemplate = templates.First(t => t.Name == pointerMethod.Template);

            //Construct a list of all methods inside this template (and its sub-regions) for us to later analyze

            foreach (var methodDef in GetTemplatedMethodResolutionCandidatesFromRegions(targetTemplate.Regions))
                yield return methodDef;

            foreach (var methodDef in targetTemplate.MethodDefs)
                yield return methodDef;
        }

        private IEnumerable<MethodDef> GetTemplatedMethodResolutionCandidatesFromRegions(IEnumerable<RegionDef> regions)
        {
            foreach (var region in regions)
            {
                foreach (var methodDef in GetTemplatedMethodResolutionCandidatesFromRegions(region.Elements.OfType<RegionDef>()))
                    yield return methodDef;

                foreach (var methodDef in region.Elements.OfType<MethodDef>())
                    yield return methodDef;
            }
        }

        #endregion
    }
}
