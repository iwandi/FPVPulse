using System;

namespace FPVPulse.Ingest
{
	public static class MergeUtil
	{
		public static bool SetMember<T>(ref T? orig, T? delta) where T : struct, IComparable<T>
		{
			if (delta.HasValue != orig.HasValue || (orig.HasValue && delta.HasValue && orig.Value.CompareTo(delta.Value) != 0))
			{
				orig = delta;
				return true;
			}
			return false;
		}

		public static bool SetMember<T>(ref T? orig, T? delta) where T : class, IComparable<T>
		{
			if ((delta != null) != (orig != null) || (orig != null && delta != null && orig.CompareTo(delta) != 0))
			{
				orig = delta;
				return true;
			}
			return false;
		}

		public static bool MergeMember<T>(ref T? orig, T? delta) where T : struct, IComparable<T>
		{
			if ((!orig.HasValue && delta.HasValue) ||
				(delta.HasValue && delta.Value.CompareTo(orig.Value) != 0))
			{
				orig = delta;
				return true;
			}
			return false;
		}

		public static bool MergeMember<T>(ref T orig, T? delta) where T : struct, IComparable<T>
		{
			if (delta.HasValue && orig.CompareTo(delta.Value) != 0)
			{
				orig = delta.Value;
				return true;
			}
			return false;
		}

		public static bool MergeEnumMember<T>(ref T? orig, T? delta) where T : struct, IComparable
		{
			if ((!orig.HasValue && delta.HasValue) ||
				(delta.HasValue && delta.Value.CompareTo(orig.Value) != 0))
			{
				orig = delta;
				return true;
			}
			return false;
		}

		public static bool MergeEnumMember<T>(ref T orig, T? delta) where T : struct, IComparable
		{
			if (delta.HasValue && orig.CompareTo(delta.Value) != 0)
			{
				orig = delta.Value;
				return true;
			}
			return false;
		}

		public static bool MergeMember<T>(ref T orig, T delta) where T : class, IComparable<T>
		{
			if ((orig == null && delta != null) ||
				(delta != null && delta.CompareTo(orig) != 0))
			{
				orig = delta;
				return true;
			}
			return false;
		}

		public static bool MergeMemberNullableString(ref string? orig,  string? delta)
		{
			bool origHasValue = orig != null;
			bool deltaHasValue = string.IsNullOrWhiteSpace(delta);
			if ((!origHasValue && deltaHasValue) ||
				(deltaHasValue && delta != orig))
			{
				orig = delta;
				return true;
			}
			return false;
		}

		public static bool MergeMemberString(ref string orig, string? delta)
		{
			bool deltaHasValue = string.IsNullOrWhiteSpace(delta);
			if (deltaHasValue && delta != orig)
			{
				orig = delta;
				return true;
			}
			return false;
		}
	}
}
