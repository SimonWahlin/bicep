// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Bicep.Core.Diagnostics;
using Bicep.Core.UnitTests;
using Bicep.Core.UnitTests.Assertions;
using Bicep.Core.UnitTests.Utils;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Bicep.Core.IntegrationTests
{
    [TestClass]
    public class DecoratorTests
    {
        private static ServiceBuilder Services => new ServiceBuilder().WithEmptyAzResources();

        [TestMethod]
        public void DeprecatedDecorator_EmitsWarning()
        {
            var (template, diagnostics, _) = CompilationHelper.Compile(@"
@deprecated('This parameter is deprecated.')
param deprecatedParam string = 'default'
");

            using (new AssertionScope())
            {
                diagnostics.Should().HaveDiagnostics(new[] {
                    ("BCPXXX", DiagnosticLevel.Warning, "The parameter 'deprecatedParam' is deprecated. This parameter is deprecated."),
                });
            }
        }

        [TestMethod]
        public void DeprecatedDecorator_WithoutMessage_EmitsWarning()
        {
            var (template, diagnostics, _) = CompilationHelper.Compile(@"
@deprecated()
param deprecatedParam string = 'default'
");

            using (new AssertionScope())
            {
                diagnostics.Should().HaveDiagnostics(new[] {
                    ("BCPXXX", DiagnosticLevel.Warning, "The parameter 'deprecatedParam' is deprecated."),
                });
            }
        }

        [TestMethod]
        public void DeprecatedDecorator_OnNonParameter_DoesNotEmitWarning()
        {
            var (template, diagnostics, _) = CompilationHelper.Compile(@"
@deprecated('This variable is deprecated.')
var deprecatedVar = 'value'
");

            using (new AssertionScope())
            {
                diagnostics.Should().NotHaveDiagnostics(new[] {
                    ("BCPXXX", DiagnosticLevel.Warning, "The variable 'deprecatedVar' is deprecated."),
                });
            }
        }
    }
}
