using System;
using System.Collections;
using System.Linq;

namespace ELTE.EVA.Lecture02.VectorCalculation
{
    public class Vector : IEquatable<Vector>
    {
        #region Vector properties

        public Int32 Dimension { get; }

        public Int32[] Elements { get; }

        public Vector(params Int32[] elements)
        {
            Elements = elements;
            Dimension = elements.Length;
        }

        #endregion

        #region Static construct methods

        public static Vector Zero(Int32 dimension)
        {
            return Create(dimension, 0);
        }

        public static Vector Create(Int32 dimension, Int32 defaultValue = 0)
        {
            return new Vector(Enumerable.Repeat(defaultValue, dimension).ToArray());
        }

        #endregion

        #region Mathematical operations

        public Vector Add(Vector other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            if (Dimension != other.Dimension)
                throw new Exception($"{nameof(Dimension)} is not matching.");

            var elements = Elements;
            for (var i = 0; i < Dimension; i++)
            {
                elements[i] += other.Elements[i];
            }

            return new Vector(elements);
        }

        public Double GetLength()
        {
            var length = 0.0;
            foreach (var element in Elements)
            {
                length += element*element;
            }
            return Math.Sqrt(length);
        }

        #endregion
        
        #region Equality members

        public static Boolean operator ==(Vector that, Vector other)
        {
            return that?.Equals(other) ?? ReferenceEquals(other, null);
        }

        public static Boolean operator !=(Vector that, Vector other)
        {
            return !(that == other);
        }

        public Boolean Equals(Vector other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Elements.SequenceEqual(other.Elements);
        }

        public override Boolean Equals(Object obj)
        {
            return Equals(obj as Vector);
        }

        public override Int32 GetHashCode()
        {
            return (Elements != null ? Elements.GetHashCode() : 0);
        }

        #endregion
    }
}