// <copyright file="IoCTest.cs" company="Gardiner Family">Copyright © Gardiner Family 2009</copyright>
using System;
using CastleExperiments;
using MbUnit.Framework;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;

namespace CastleExperiments
{
    /// <summary>This class contains parameterized unit tests for IoC</summary>
    [TestFixture]
    [PexClass(typeof(IoC))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    public partial class IoCTest
    {
        /// <summary>Test stub for Resolve()</summary>
        [PexGenericArguments(typeof(int))]
        [PexMethod]
        public T Resolve<T>()
        {
            // TODO: add assertions to method IoCTest.Resolve()
            T result = IoC.Resolve<T>();
            return result;
        }
    }
}
