// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: The structure for holding all of the data needed
**          for object serialization and deserialization.
**
**
===========================================================*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using System.Security;
using System.Runtime.CompilerServices;

namespace System.Runtime.Serialization
{
    public sealed class SerializationInfo
    {
        private const int DefaultSize = 4;
        private const string s_mscorlibAssemblySimpleName = System.CoreLib.Name;
        private const string s_mscorlibFileName = s_mscorlibAssemblySimpleName + ".dll";

        // Even though we have a dictionary, we're still keeping all the arrays around for back-compat. 
        // Otherwise we may run into potentially breaking behaviors like GetEnumerator() not returning entries in the same order they were added.
        internal string[] _names;
        internal object[] _values;
        internal Type[] _types;
        private Dictionary<string, int> _nameToIndex;
        internal int _count;
        internal IFormatterConverter _converter;
        private string _rootTypeName;
        private string _rootTypeAssemblyName;
        private Type _rootType;
        private bool requireSameTokenInPartialTrust;

        [CLSCompliant(false)]
        public SerializationInfo(Type type, IFormatterConverter converter)
            : this(type, converter, false)
        {
        }

        [CLSCompliant(false)]
        public SerializationInfo(Type type, IFormatterConverter converter, bool requireSameTokenInPartialTrust)
        {
            if ((object)type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            _rootType = type;
            _rootTypeName = type.FullName;
            _rootTypeAssemblyName = type.Module.Assembly.FullName;

            _names = new string[DefaultSize];
            _values = new object[DefaultSize];
            _types = new Type[DefaultSize];

            _nameToIndex = new Dictionary<string, int>();

            _converter = converter;

            this.requireSameTokenInPartialTrust = requireSameTokenInPartialTrust;
        }

        public string FullTypeName
        {
            get { return _rootTypeName; }
            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _rootTypeName = value;
                IsFullTypeNameSetExplicit = true;
            }
        }

        public string AssemblyName
        {
            get
            {
                return _rootTypeAssemblyName;
            }
            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (requireSameTokenInPartialTrust)
                {
                    DemandForUnsafeAssemblyNameAssignments(_rootTypeAssemblyName, value);
                }
                _rootTypeAssemblyName = value;
                IsAssemblyNameSetExplicit = true;
            }
        }

        public bool IsFullTypeNameSetExplicit { get; private set; }

        public bool IsAssemblyNameSetExplicit { get; private set; }

        public void SetType(Type type)
        {
            if ((object)type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (requireSameTokenInPartialTrust)
            {
                DemandForUnsafeAssemblyNameAssignments(this.ObjectType.Assembly.FullName, type.Assembly.FullName);
            }

            if (!object.ReferenceEquals(_rootType, type))
            {
                _rootType = type;
                _rootTypeName = type.FullName;
                _rootTypeAssemblyName = type.Module.Assembly.FullName;
                IsFullTypeNameSetExplicit = false;
                IsAssemblyNameSetExplicit = false;
            }
        }

        internal static void DemandForUnsafeAssemblyNameAssignments(string originalAssemblyName, string newAssemblyName)
        {
        }

        public int MemberCount => _count;

        public Type ObjectType => _rootType;

        public SerializationInfoEnumerator GetEnumerator() => new SerializationInfoEnumerator(_names, _values, _types, _count);

        private void ExpandArrays()
        {
            int newSize;
            Debug.Assert(_names.Length == _count, "[SerializationInfo.ExpandArrays]_names.Length == _count");

            newSize = (_count * 2);

            //
            // In the pathological case, we may wrap
            //
            if (newSize < _count)
            {
                if (Int32.MaxValue > _count)
                {
                    newSize = Int32.MaxValue;
                }
            }

            //
            // Allocate more space and copy the data
            //
            string[] newMembers = new string[newSize];
            object[] newData = new object[newSize];
            Type[] newTypes = new Type[newSize];

            Array.Copy(_names, newMembers, _count);
            Array.Copy(_values, newData, _count);
            Array.Copy(_types, newTypes, _count);

            //
            // Assign the new arrys back to the member vars.
            //
            _names = newMembers;
            _values = newData;
            _types = newTypes;
        }

        public void AddValue(string name, object value, Type type)
        {
            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if ((object)type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            AddValueInternal(name, value, type);
        }

        public void AddValue(string name, object value)
        {
            if (null == value)
            {
                AddValue(name, value, typeof(object));
            }
            else
            {
                AddValue(name, value, value.GetType());
            }
        }

        public void AddValue(string name, bool value)
        {
            AddValue(name, (object)value, typeof(bool));
        }

        public void AddValue(string name, char value)
        {
            AddValue(name, (object)value, typeof(char));
        }

        [CLSCompliant(false)]
        public void AddValue(string name, sbyte value)
        {
            AddValue(name, (object)value, typeof(sbyte));
        }

        public void AddValue(string name, byte value)
        {
            AddValue(name, (object)value, typeof(byte));
        }

        public void AddValue(string name, short value)
        {
            AddValue(name, (object)value, typeof(short));
        }

        [CLSCompliant(false)]
        public void AddValue(string name, ushort value)
        {
            AddValue(name, (object)value, typeof(ushort));
        }

        public void AddValue(string name, int value)
        {
            AddValue(name, (object)value, typeof(int));
        }

        [CLSCompliant(false)]
        public void AddValue(string name, uint value)
        {
            AddValue(name, (object)value, typeof(uint));
        }

        public void AddValue(string name, long value)
        {
            AddValue(name, (object)value, typeof(long));
        }

        [CLSCompliant(false)]
        public void AddValue(string name, ulong value)
        {
            AddValue(name, (object)value, typeof(ulong));
        }

        public void AddValue(string name, float value)
        {
            AddValue(name, (object)value, typeof(float));
        }

        public void AddValue(string name, double value)
        {
            AddValue(name, (object)value, typeof(double));
        }

        public void AddValue(string name, decimal value)
        {
            AddValue(name, (object)value, typeof(decimal));
        }

        public void AddValue(string name, DateTime value)
        {
            AddValue(name, (object)value, typeof(DateTime));
        }

        internal void AddValueInternal(string name, object value, Type type)
        {
            if (_nameToIndex.ContainsKey(name))
            {
                throw new SerializationException(SR.Serialization_SameNameTwice);
            }
            _nameToIndex.Add(name, _count);

            // If we need to expand the arrays, do so.
            if (_count >= _names.Length)
            {
                ExpandArrays();
            }

            // Add the data and then advance the counter.
            _names[_count] = name;
            _values[_count] = value;
            _types[_count] = type;
            _count++;
        }

        /*=================================UpdateValue==================================
        **Action: Finds the value if it exists in the current data.  If it does, we replace
        **        the values, if not, we append it to the end.  This is useful to the 
        **        ObjectManager when it's performing fixups.
        **Returns: void
        **Arguments: name  -- the name of the data to be updated.
        **           value -- the new value.
        **           type  -- the type of the data being added.
        **Exceptions: None.  All error checking is done with asserts. Although public in coreclr,
        **            it's not exposed in a contract and is only meant to be used by corefx.
        ==============================================================================*/
        // This should not be used by clients: exposing out this functionality would allow children
        // to overwrite their parent's values. It is public in order to give corefx access to it for
        // its ObjectManager implementation, but it should not be exposed out of a contract.
        public void UpdateValue(string name, object value, Type type)
        {
            Debug.Assert(null != name, "[SerializationInfo.UpdateValue]name!=null");
            Debug.Assert(null != value, "[SerializationInfo.UpdateValue]value!=null");
            Debug.Assert(null != (object)type, "[SerializationInfo.UpdateValue]type!=null");

            int index = FindElement(name);
            if (index < 0)
            {
                AddValueInternal(name, value, type);
            }
            else
            {
                _values[index] = value;
                _types[index] = type;
            }
        }

        private int FindElement(string name)
        {
            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }
            int index;
            if (_nameToIndex.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }

        /*==================================GetElement==================================
        **Action: Use FindElement to get the location of a particular member and then return
        **        the value of the element at that location.  The type of the member is
        **        returned in the foundType field.
        **Returns: The value of the element at the position associated with name.
        **Arguments: name -- the name of the element to find.
        **           foundType -- the type of the element associated with the given name.
        **Exceptions: None.  FindElement does null checking and throws for elements not 
        **            found.
        ==============================================================================*/
        private object GetElement(string name, out Type foundType)
        {
            int index = FindElement(name);
            if (index == -1)
            {
                throw new SerializationException(SR.Format(SR.Serialization_NotFound, name));
            }

            Debug.Assert(index < _values.Length, "[SerializationInfo.GetElement]index<_values.Length");
            Debug.Assert(index < _types.Length, "[SerializationInfo.GetElement]index<_types.Length");

            foundType = _types[index];
            Debug.Assert((object)foundType != null, "[SerializationInfo.GetElement]foundType!=null");
            return _values[index];
        }

        private object GetElementNoThrow(string name, out Type foundType)
        {
            int index = FindElement(name);
            if (index == -1)
            {
                foundType = null;
                return null;
            }

            Debug.Assert(index < _values.Length, "[SerializationInfo.GetElement]index<_values.Length");
            Debug.Assert(index < _types.Length, "[SerializationInfo.GetElement]index<_types.Length");

            foundType = _types[index];
            Debug.Assert((object)foundType != null, "[SerializationInfo.GetElement]foundType!=null");
            return _values[index];
        }

        //
        // The user should call one of these getters to get the data back in the 
        // form requested.  
        //

        public object GetValue(string name, Type type)
        {
            if ((object)type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            RuntimeType rt = type as RuntimeType;
            if (rt == null)
                throw new ArgumentException(SR.Argument_MustBeRuntimeType);

            Type foundType;
            object value;

            value = GetElement(name, out foundType);

            if (object.ReferenceEquals(foundType, type) || type.IsAssignableFrom(foundType) || value == null)
            {
                return value;
            }

            Debug.Assert(_converter != null, "[SerializationInfo.GetValue]_converter!=null");

            return _converter.Convert(value, type);
        }

        internal object GetValueNoThrow(string name, Type type)
        {
            Type foundType;
            object value;

            Debug.Assert((object)type != null, "[SerializationInfo.GetValue]type ==null");
            Debug.Assert(type is RuntimeType, "[SerializationInfo.GetValue]type is not a runtime type");

            value = GetElementNoThrow(name, out foundType);
            if (value == null)
                return null;

            if (object.ReferenceEquals(foundType, type) || type.IsAssignableFrom(foundType) || value == null)
            {
                return value;
            }

            Debug.Assert(_converter != null, "[SerializationInfo.GetValue]_converter!=null");

            return _converter.Convert(value, type);
        }

        public bool GetBoolean(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(bool)))
            {
                return (bool)value;
            }
            return _converter.ToBoolean(value);
        }

        public char GetChar(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(char)))
            {
                return (char)value;
            }
            return _converter.ToChar(value);
        }

        [CLSCompliant(false)]
        public sbyte GetSByte(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(sbyte)))
            {
                return (sbyte)value;
            }
            return _converter.ToSByte(value);
        }

        public byte GetByte(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(byte)))
            {
                return (byte)value;
            }
            return _converter.ToByte(value);
        }

        public short GetInt16(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(short)))
            {
                return (short)value;
            }
            return _converter.ToInt16(value);
        }

        [CLSCompliant(false)]
        public ushort GetUInt16(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(ushort)))
            {
                return (ushort)value;
            }
            return _converter.ToUInt16(value);
        }

        public int GetInt32(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(int)))
            {
                return (int)value;
            }
            return _converter.ToInt32(value);
        }

        [CLSCompliant(false)]
        public uint GetUInt32(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(uint)))
            {
                return (uint)value;
            }
            return _converter.ToUInt32(value);
        }

        public long GetInt64(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(long)))
            {
                return (long)value;
            }
            return _converter.ToInt64(value);
        }

        [CLSCompliant(false)]
        public ulong GetUInt64(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(ulong)))
            {
                return (ulong)value;
            }
            return _converter.ToUInt64(value);
        }

        public float GetSingle(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(float)))
            {
                return (float)value;
            }
            return _converter.ToSingle(value);
        }


        public double GetDouble(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(double)))
            {
                return (double)value;
            }
            return _converter.ToDouble(value);
        }

        public decimal GetDecimal(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(decimal)))
            {
                return (decimal)value;
            }
            return _converter.ToDecimal(value);
        }

        public DateTime GetDateTime(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(DateTime)))
            {
                return (DateTime)value;
            }
            return _converter.ToDateTime(value);
        }

        public string GetString(string name)
        {
            Type foundType;
            object value;

            value = GetElement(name, out foundType);
            if (object.ReferenceEquals(foundType, typeof(string)) || value == null)
            {
                return (string)value;
            }
            return _converter.ToString(value);
        }
    }
}
