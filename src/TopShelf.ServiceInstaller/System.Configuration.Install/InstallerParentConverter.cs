using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Install
{
    internal class InstallerParentConverter : ReferenceConverter
    {
        public InstallerParentConverter(Type type)
            : base(type)
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            StandardValuesCollection standardValues = base.GetStandardValues(context);
            object instance = context.Instance;
            int i = 0;
            int num = 0;
            object[] array = new object[standardValues.Count - 1];
            for (; i < standardValues.Count; i++)
            {
                if (standardValues[i] != instance)
                {
                    array[num] = standardValues[i];
                    num++;
                }
            }
            return new StandardValuesCollection(array);
        }
    }
}
