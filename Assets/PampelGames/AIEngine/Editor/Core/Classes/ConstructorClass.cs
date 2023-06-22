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
    public class ConstructorClass
    {
        public string className;
        public Type classType;
        public List<Type> parameterTypes;
        public object[] parameters;
        public ConstructorInfo constructorInfo;
    }
}