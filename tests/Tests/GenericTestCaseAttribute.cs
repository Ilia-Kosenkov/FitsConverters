#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class GenericTestCaseAttribute : TestCaseAttribute, ITestBuilder
    {
        public GenericTestCaseAttribute(params object[] arguments)
            : base(arguments)
        {
        }


        public Type[]? TypeArguments { get; set; }

        IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test suite)
        {
            if (!method.IsGenericMethodDefinition)
                return BuildFrom(method, suite);

            if (TypeArguments is null || TypeArguments.Length != method.GetGenericArguments().Length)
            {
                var @params = new TestCaseParameters { RunState = RunState.NotRunnable };
                @params.Properties.Set(@"_SKIPREASON", $"{nameof(TypeArguments)} should have {method.GetGenericArguments().Length} elements.");
                return new[] { new NUnitTestCaseBuilder().BuildTestMethod(method, suite, @params) };
            }

            var genMethod = method.MakeGenericMethod(TypeArguments);
            return BuildFrom(genMethod, suite);
        }
    }
}
