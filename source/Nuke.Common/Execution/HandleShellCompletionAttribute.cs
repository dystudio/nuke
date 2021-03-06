// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.IO;

namespace Nuke.Common.Execution
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class HandleShellCompletionAttribute : Attribute, IOnBeforeLogo
    {
        public void OnBeforeLogo(
            NukeBuild build,
            IReadOnlyCollection<ExecutableTarget> executableTargets)
        {
            var completionItems = new SortedDictionary<string, string[]>();

            var targetNames = build.ExecutableTargets.Select(x => x.Name).OrderBy(x => x).ToList();
            completionItems[Constants.InvokedTargetsParameterName] = targetNames.ToArray();
            completionItems[Constants.SkippedTargetsParameterName] = targetNames.ToArray();

            var parameters = InjectionUtility.GetParameterMembers(build.GetType(), includeUnlisted: false);
            foreach (var parameter in parameters)
            {
                var parameterName = ParameterService.GetParameterMemberName(parameter);
                if (completionItems.ContainsKey(parameterName))
                    continue;

                var subItems = ParameterService.GetParameterValueSet(parameter, build)?.Select(x => x.Text);
                completionItems[parameterName] = subItems?.ToArray();
            }

            SerializationTasks.YamlSerializeToFile(completionItems, Constants.GetCompletionFile(NukeBuild.RootDirectory));

            if (EnvironmentInfo.GetParameter<bool>(Constants.CompletionParameterName))
                Environment.Exit(exitCode: 0);
        }
    }
}
