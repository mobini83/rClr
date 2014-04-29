﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rclr;
using Xunit;

namespace RclrTests
{
    public class ReflectionTests
    {

        private class MyTestClass
        {
            public void NoParams() { }
            public void IntParams(params int []p) { }
            public void DoubleIntParams(double d, params int[] p) { }

            public void StringSameNameParams(string s, params int[] p) { }
            public void StringSameNameParams(string s, params double[] p) { }

            public void NoDiffSameNameParams(params int[] p) { }
            public void NoDiffSameNameParams(params double[] p) { }

            public void OptionalInt(int i = 0) { }
            public void IntOptionalInt(int blah, int i = 0) { }
            public void DoubleOptionalInt(double blah, int i = 0) { }
            public void DoubleOptionalIntDoubleString(double blah, int i = 0, double d2 = 5.6, string tag="tag") { }

        }

        [Fact]
        public void TestVariableArgumentMethodBinding()
        {
            BindingFlags sbf = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod;
            BindingFlags bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod;
            var t = typeof(MyTestClass);
            Assert.False(ReflectionHelper.HasVarArgs(GetSingleMethod(t, "NoParams", bf)));
            Assert.True(ReflectionHelper.HasVarArgs(GetSingleMethod(t, "IntParams", bf)));

            var tint = typeof(int);
            var td = typeof(double);
            var to = typeof(object);
            var ts = typeof(string);

            Assert.NotNull(ReflectionHelper.GetMethod(t, "IntParams", null, bf, new[] { tint, tint, tint, tint }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "IntParams", null, bf, new[] { tint }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "IntParams", null, bf, Type.EmptyTypes));
            Assert.Null(ReflectionHelper.GetMethod(t, "IntParams", null, bf, new[] { to, to }));

            Assert.NotNull(ReflectionHelper.GetMethod(t, "StringSameNameParams", null, bf, new[] { ts, tint, tint, tint }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "StringSameNameParams", null, bf, new[] { ts, td, td, td }));

            Assert.NotNull(ReflectionHelper.GetMethod(t, "NoDiffSameNameParams", null, bf, new[] { tint, tint, tint }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "NoDiffSameNameParams", null, bf, new[] { td, td, td }));
        }

        [Fact]
        public void TestOptionalParametersMethodBinding()
        {
            BindingFlags bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod;
            var t = typeof(MyTestClass);
            Assert.False(ReflectionHelper.HasOptionalParams(GetSingleMethod(t, "NoParams", bf)));
            Assert.True(ReflectionHelper.HasOptionalParams(GetSingleMethod(t, "OptionalInt", bf)));
            Assert.True(ReflectionHelper.HasOptionalParams(GetSingleMethod(t, "IntOptionalInt", bf)));

            var tint = typeof(int);
            var td = typeof(double);
            var to = typeof(object);
            var ts = typeof(string);

            Assert.NotNull(ReflectionHelper.GetMethod(t, "OptionalInt", null, bf, new[] { tint }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "OptionalInt", null, bf, Type.EmptyTypes));
            Assert.Null(ReflectionHelper.GetMethod(t, "OptionalInt", null, bf, new[] { tint, tint }));
            Assert.Null(ReflectionHelper.GetMethod(t, "OptionalInt", null, bf, new[] { td, tint }));
            Assert.Null(ReflectionHelper.GetMethod(t, "OptionalInt", null, bf, new[] { tint, td }));

            Assert.NotNull(ReflectionHelper.GetMethod(t, "IntOptionalInt", null, bf, new[] { tint }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "IntOptionalInt", null, bf, new[] { tint, tint }));
            Assert.Null(ReflectionHelper.GetMethod(t, "IntOptionalInt", null, bf, new[] { tint, tint, tint }));

            Assert.NotNull(ReflectionHelper.GetMethod(t, "DoubleOptionalInt", null, bf, new[] { td }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "DoubleOptionalInt", null, bf, new[] { td, tint }));
            Assert.Null(ReflectionHelper.GetMethod(t, "DoubleOptionalInt", null, bf, new[] { td, tint, tint }));

            Assert.NotNull(ReflectionHelper.GetMethod(t, "DoubleOptionalIntDoubleString", null, bf, new[] { td, tint, td, ts }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "DoubleOptionalIntDoubleString", null, bf, new[] { td, tint, td }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "DoubleOptionalIntDoubleString", null, bf, new[] { td, tint }));
            Assert.NotNull(ReflectionHelper.GetMethod(t, "DoubleOptionalIntDoubleString", null, bf, new[] { td }));

            Assert.Null(ReflectionHelper.GetMethod(t, "DoubleOptionalIntDoubleString", null, bf, new[] { td, tint, td, ts, to }));
            Assert.Null(ReflectionHelper.GetMethod(t, "DoubleOptionalIntDoubleString", null, bf, new[] { td, tint, td, to }));
            Assert.Null(ReflectionHelper.GetMethod(t, "DoubleOptionalIntDoubleString", null, bf, new[] { td, to }));
            Assert.Null(ReflectionHelper.GetMethod(t, "DoubleOptionalIntDoubleString", null, bf, Type.EmptyTypes));

        }

        [Fact]
        public void TestOptionalParametersMethodInvocation()
        {
            // TODO tighter checks. Start with: it does not bomb...
            var obj = new MyTestClass();
            ClrFacade.CallInstanceMethod(obj, "OptionalInt", new object[] { });
            ClrFacade.CallInstanceMethod(obj, "OptionalInt", new object[] { 3 });

            ClrFacade.CallInstanceMethod(obj, "IntOptionalInt", new object[] { 3 });
            ClrFacade.CallInstanceMethod(obj, "IntOptionalInt", new object[] { 3, 5 });

            ClrFacade.CallInstanceMethod(obj, "DoubleOptionalInt", new object[] { 3.0 });
            ClrFacade.CallInstanceMethod(obj, "DoubleOptionalInt", new object[] { 3.0, 5 });

            ClrFacade.CallInstanceMethod(obj, "DoubleOptionalIntDoubleString", new object[] { 3.0, 5, 4.5, "blah" });
            ClrFacade.CallInstanceMethod(obj, "DoubleOptionalIntDoubleString", new object[] { 3.0, 5, 4.5 });
            ClrFacade.CallInstanceMethod(obj, "DoubleOptionalIntDoubleString", new object[] { 3.0, 5 });
            ClrFacade.CallInstanceMethod(obj, "DoubleOptionalIntDoubleString", new object[] { 3.0 });
        }

        private MethodInfo GetSingleMethod(Type classType, string methodName, BindingFlags bf, Binder binder=null, Type[] types=null)
        {
            if (types == null) types = System.Type.EmptyTypes;
            var method = classType.GetMethod(methodName, bf, binder, types, null);
            if (method != null) return method;
            return classType.GetMethod(methodName);
        }
    }
}
