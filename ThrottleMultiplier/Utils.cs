using System;
using System.Collections.Generic;
using System.Reflection;
//using System.Threading.Tasks;
using UnityEngine;

namespace ThrottleMultiplier {
	public static class MuUtils {
		public static float ResourceDensity(int type) {
			return PartResourceLibrary.Instance.GetDefinition(type).density;
		}

		//Puts numbers into SI format, e.g. 1234 -> "1.234 k", 0.0045678 -> "4.568 m"
		//maxPrecision is the exponent of the smallest place value that will be shown; for example
		//if maxPrecision = -1 and digitsAfterDecimal = 3 then 12.345 will be formatted as "12.3"
		//while 56789 will be formated as "56.789 k"
		public static string ToSI(double d, int maxPrecision = -99, int sigFigs = 4) {
			if (d == 0 || double.IsInfinity(d) || double.IsNaN(d)) return d.ToString() + " ";

			int exponent = (int)Math.Floor(Math.Log10(Math.Abs(d))); //exponent of d if it were expressed in scientific notation

			string[] units = new string[] { "y", "z", "a", "f", "p", "n", "μ", "m", "", "k", "M", "G", "T", "P", "E", "Z", "Y" };
			const int unitIndexOffset = 8; //index of "" in the units array
			int unitIndex = (int)Math.Floor(exponent / 3.0) + unitIndexOffset;
			if (unitIndex < 0) unitIndex = 0;
			if (unitIndex >= units.Length) unitIndex = units.Length - 1;
			string unit = units[unitIndex];

			int actualExponent = (unitIndex - unitIndexOffset) * 3; //exponent of the unit we will us, e.g. 3 for k.
			d /= Math.Pow(10, actualExponent);

			int digitsAfterDecimal = sigFigs - (int)(Math.Ceiling(Math.Log10(Math.Abs(d))));

			if (digitsAfterDecimal > actualExponent - maxPrecision) digitsAfterDecimal = actualExponent - maxPrecision;
			if (digitsAfterDecimal < 0) digitsAfterDecimal = 0;

			string ret = d.ToString("F" + digitsAfterDecimal) + " " + unit;

			return ret;
		}

		public static string PadPositive(double x, string format = "F3") {
			string s = x.ToString(format);
			return s[0] == '-' ? s : " " + s;
		}

		public static string PrettyPrint(Vector3d vector, string format = "F3") {
			return "[" + PadPositive(vector.x, format) + ", " + PadPositive(vector.y, format) + ", " + PadPositive(vector.z, format) + " ]";
		}

		public static string PrettyPrint(Quaternion quaternion, string format = "F3") {
			return "[" + PadPositive(quaternion.x, format) + ", " + PadPositive(quaternion.y, format) + ", " + PadPositive(quaternion.z, format) + ", " + PadPositive(quaternion.w, format) + "]";
		}

		//For some reason, Math doesn't have the inverse hyperbolic trigonometric functions:
		//asinh(x) = log(x + sqrt(x^2 + 1))
		public static double Asinh(double x) {
			return Math.Log(x + Math.Sqrt(x * x + 1));
		}

		//acosh(x) = log(x + sqrt(x^2 - 1))
		public static double Acosh(double x) {
			return Math.Log(x + Math.Sqrt(x * x - 1));
		}

		//atanh(x) = (log(1+x) - log(1-x))/2
		public static double Atanh(double x) {
			return 0.5 * (Math.Log(1 + x) - Math.Log(1 - x));
		}

		//since there doesn't seem to be a Math.Clamp?
		public static double Clamp(double x, double min, double max) {
			if (x < min) return min;
			if (x > max) return max;
			return x;
		}

		//keeps angles in the range 0 to 360
		public static double ClampDegrees360(double angle) {
			angle = angle % 360.0;
			if (angle < 0) return angle + 360.0;
			else return angle;
		}

		//keeps angles in the range -180 to 180
		public static double ClampDegrees180(double angle) {
			angle = ClampDegrees360(angle);
			if (angle > 180) angle -= 360;
			return angle;
		}

		public static double ClampRadiansTwoPi(double angle) {
			angle = angle % (2 * Math.PI);
			if (angle < 0) return angle + 2 * Math.PI;
			else return angle;
		}

		public static double ClampRadiansPi(double angle) {
			angle = ClampRadiansTwoPi(angle);
			if (angle > Math.PI) angle -= 2 * Math.PI;
			return angle;
		}

		public static bool PhysicsRunning() {
			return (TimeWarp.WarpMode == TimeWarp.Modes.LOW) || (TimeWarp.CurrentRateIndex == 0);
		}

		//Some black magic to access the system clipboard from within Unity, found somewhere on the Web.
		//Unfortunately it doesn't seem we have access to the System.Windows.Forms.Clipboard class, which would 
		//make this easier.
		private static PropertyInfo m_systemCopyBufferProperty = null;
		private static PropertyInfo GetSystemCopyBufferProperty() {
			if (m_systemCopyBufferProperty == null) {
				Type T = typeof(GUIUtility);
				m_systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
				if (m_systemCopyBufferProperty == null)
					throw new Exception("Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
			}
			return m_systemCopyBufferProperty;
		}
		public static string SystemClipboard {
			get {
				PropertyInfo P = GetSystemCopyBufferProperty();
				return (string)P.GetValue(null, null);
			}
			set {
				PropertyInfo P = GetSystemCopyBufferProperty();
				P.SetValue(null, value, null);
			}
		}

		public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB) {
			T tmp = list[indexA];
			list[indexA] = list[indexB];
			list[indexB] = tmp;
			return list;
		}

		public static void DrawLine(Texture2D tex, int x1, int y1, int x2, int y2, Color col) {
			int dy = y2 - y1;
			int dx = x2 - x1;
			int stepx, stepy;

			if (dy < 0) { dy = -dy; stepy = -1; }
			else { stepy = 1; }
			if (dx < 0) { dx = -dx; stepx = -1; }
			else { stepx = 1; }
			dy <<= 1;
			dx <<= 1;

			float fraction = 0;

			tex.SetPixel(x1, y1, col);
			if (dx > dy) {
				fraction = dy - (dx >> 1);
				while (Mathf.Abs(x1 - x2) > 1) {
					if (fraction >= 0) {
						y1 += stepy;
						fraction -= dx;
					}
					x1 += stepx;
					fraction += dy;
					tex.SetPixel(x1, y1, col);
				}
			}
			else {
				fraction = dx - (dy >> 1);
				while (Mathf.Abs(y1 - y2) > 1) {
					if (fraction >= 0) {
						x1 += stepx;
						fraction -= dy;
					}
					y1 += stepy;
					fraction += dx;
					tex.SetPixel(x1, y1, col);
				}
			}
		}
		public static Color HSVtoRGB(float hue, float saturation, float value, float alpha) {
			int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
			float f = hue / 60 - Mathf.Floor(hue / 60);

			float v = value;
			float p = value * (1 - saturation);
			float q = value * (1 - f * saturation);
			float t = value * (1 - (1 - f) * saturation);

			if (hi == 0)
				return new Color(v, t, p, alpha);
			if (hi == 1)
				return new Color(q, v, p, alpha);
			if (hi == 2)
				return new Color(p, v, t, alpha);
			if (hi == 3)
				return new Color(p, q, v, alpha);
			if (hi == 4)
				return new Color(t, p, v, alpha);

			return new Color(v, p, q, alpha);
		}
	}

	public class EditableValue
    {
        [Persistent]
        public double value = 0;
        [Persistent]
        public bool locked = false;
        [Persistent]
        public bool initialized = false;

        [Persistent]
        public bool boolValue = true;

        public bool valid = true;
        public String format = "{0:0.00}";
        public string str = "0";


        public EditableValue(double initialValue, string printFormat = "{0:0.00}", bool locked = false)
        {
            if (!initialized)
            {
                this.locked = locked;
                value = initialValue;
                initialized = true;
            }
            format = printFormat;
            valid = true;
            str = value.ToString();
        }

        public EditableValue(string initialStr, string printFormat = "{0:0.00}", bool locked = false)
        {
            this.locked = locked;
            if (!initialized)
            {
                format = printFormat;
                initialized = true;
            }
            try
            {
                value = double.Parse(initialStr);
                str = initialStr;
                valid = true;
            }
            catch (Exception)
            {
                value = 0;
                str = initialStr;
                valid = false;
            }
        }

        public EditableValue(bool initialBool, bool locked = false)
        {
            this.locked = locked;
            if (!initialized)
            {
                format = "";
                initialized = true;
            }
            boolValue = initialBool;
            str = value.ToString();
            valid = true;
        }

        public void setValue(bool b)
        {
            boolValue = b;
            str = b.ToString();
            valid = true;
        }

        public static implicit operator bool(EditableValue e)
        {
            return e.boolValue;
        }


        public void setValue(string s)
        {
            try
            {
                value = double.Parse(s);
                str = s;
                valid = true;
            }
            catch (Exception)
            {
                str = s;
                valid = false;
            }
        }


        public override string ToString()
        {
            if (valid)
                return string.Format(format, value);
            else
                return str;
        }

        public static implicit operator string(EditableValue e)
        {
            return e.ToString();
        }

        public static implicit operator EditableValue(string s)
        {
            return new EditableValue(s);
        }

        public static implicit operator EditableValue(double v)
        {
            return new EditableValue(v);
        }

        public static implicit operator EditableValue(float v)
        {
            return new EditableValue(v);
        }

        public static implicit operator double(EditableValue e)
        {
            return e.value;
        }

        public static implicit operator float(EditableValue e)
        {
            return (float)e.value;
        }

    }

    //A simple wrapper around a Dictionary, with the only change being that
    //The keys are also stored in a list so they can be iterated without allocating an IEnumerator
    class KeyableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        protected Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();
        // Also store the keys in a list so we can iterate them without allocating an IEnumerator
        protected List<TKey> k = new List<TKey>();

        public virtual TValue this[TKey key]
        {
            get
            {
                return d[key];
            }
            set
            {
                if (d.ContainsKey(key)) d[key] = value;
                else
                {
                    k.Add(key);
                    d.Add(key, value);
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            k.Add(key);
            d.Add(key, value);
        }
        public bool ContainsKey(TKey key) { return d.ContainsKey(key); }
        public ICollection<TKey> Keys { get { return d.Keys; } }
        public List<TKey> KeysList { get { return k; } }

        public bool Remove(TKey key)
        {
            return d.Remove(key) && k.Remove(key);
        }
        public bool TryGetValue(TKey key, out TValue value) { return d.TryGetValue(key, out value); }
        public ICollection<TValue> Values { get { return d.Values; } }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)d).Add(item);
            k.Add(item.Key);
        }

        public void Clear()
        {
            d.Clear();
            k.Clear();
        }
        public bool Contains(KeyValuePair<TKey, TValue> item) { return ((IDictionary<TKey, TValue>)d).Contains(item); }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { ((IDictionary<TKey, TValue>)d).CopyTo(array, arrayIndex); }
        public int Count { get { return d.Count; } }
        public bool IsReadOnly { get { return ((IDictionary<TKey, TValue>)d).IsReadOnly; } }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)d).Remove(item) && k.Remove(item.Key);
        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return d.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((System.Collections.IEnumerable)d).GetEnumerator(); }
    }


    //A simple wrapper around a Dictionary, with the only change being that
    //accessing the value of a nonexistent key returns a default value instead of an error.
    class DefaultableDictionary<TKey, TValue> : KeyableDictionary<TKey, TValue>
    {
        private readonly TValue defaultValue;

        public DefaultableDictionary(TValue defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public override TValue this[TKey key]
        {
            get
            {
                if (d.ContainsKey(key)) return d[key];
                else return defaultValue;
            }
            set
            {
                if (d.ContainsKey(key)) d[key] = value;
                else
                {
                    k.Add(key);
                    d.Add(key, value);
                }
            }
        }

    }
}
