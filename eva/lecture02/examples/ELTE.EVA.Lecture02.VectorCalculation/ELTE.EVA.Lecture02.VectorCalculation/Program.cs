using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace ELTE.EVA.Lecture02.VectorCalculation
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            Vector vector = Vector.Zero(3);
            Assert($"{nameof(vector)} is equal to Vector(0, 0, 0)", vector.Equals(new Vector(0, 0, 0)));
            Assert($"{nameof(vector)} is not equal to Vector(1, 1, 0)", !vector.Equals(new Vector(1, 1, 0)));

            Vector vector2 = new Vector(1, 2, 2);
            Assert($"{nameof(vector2)} length should be equal to 3.0", vector2.GetLength() == 3.0);

            Vector vector3 = new Vector(1, 1, 1);
            Vector vector4 = vector2.Add(vector3);
            Assert($"{nameof(vector2)} + {nameof(vector3)} is equal to Vector(2, 3, 3)", vector4.Equals(new Vector(2, 3, 3)));
            Assert($"{nameof(vector2)} + {nameof(vector3)} is equal to Vector(2, 3, 3)", vector4 == new Vector(2, 3, 3));

            Console.ReadKey();
        }

        private static void Assert(String description, Boolean expectedValue)
        {
            ConsoleColor previousColour = Console.BackgroundColor;
            if (!expectedValue)
            {
                Console.BackgroundColor = ConsoleColor.Red;
            }

            Console.WriteLine("{0}: {1}", description, expectedValue);

            if (!expectedValue)
            {
                Console.BackgroundColor = previousColour;
            }
        }
    }
}