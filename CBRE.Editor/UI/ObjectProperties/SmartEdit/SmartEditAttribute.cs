using System;
using CBRE.DataStructures.GameData;

namespace CBRE.Editor.UI.ObjectProperties.SmartEdit
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class SmartEditAttribute : Attribute
    {
        public VariableType VariableType { get; set; }

        public SmartEditAttribute(VariableType variableType)
        {
            VariableType = variableType;
        }
    }
}