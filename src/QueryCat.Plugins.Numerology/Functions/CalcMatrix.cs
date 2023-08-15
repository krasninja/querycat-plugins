using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Numerology.Functions;

internal static class CalcMatrix
{
    [Description("Calculate Pythagorean matrix by date of birthday.")]
    [FunctionSignature("numerology_calc_matrix(dob: timestamp): string")]
    public static VariantValue CalcMatrixFunction(FunctionCallInfo args)
    {
        var dob = args.GetAt(0).AsTimestamp;
        var dateArr = NumberStringToArray(dob.ToString("yyyyMMdd"));

        // Additional number 1.
        var addNumber1 = dateArr.Sum();

        // Additional number 2.
        int[] addNumber2Arr;
        string addNumber2Str = ArrayToString(IntToArray(addNumber1));
        /*while ((addNumber2Arr = NumberStringToArray(addNumber2Str).Length > 0))
        {

        }*/
        return new VariantValue("test");
    }

    private static int[] NumberStringToArray(string str)
        => str.ToCharArray()
        .Select(c => int.Parse(c.ToString()))
        .ToArray();

    private static int[] IntToArray(int a)
        => NumberStringToArray(a.ToString());

    private static string ArrayToString(int[] arr)
        => string.Join(string.Empty, arr);
}
