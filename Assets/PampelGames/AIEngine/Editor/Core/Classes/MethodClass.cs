// ----------------------------------------------------
// AI Engine
// Copyright (c) Pampel Games e.K. All Rights Reserved.
// https://www.pampelgames.com
// ----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

namespace PampelGames.AIEngine.Editor
{
    public class MethodClass
    {
        public Type classType;
        public string methodName;
        public Type[] genericTypeArguments;
        public List<Type> parameterTypes;
        public string[] outRefs;
        public object[] parameters;
        public object obj;
        public MethodInfo methodInfo;
    }
}