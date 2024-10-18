using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Numerology.Functions;

internal static class CalcMatrix
{
    [SafeFunction]
    [Description("Calculate Pythagorean matrix by date of birthday.")]
    [FunctionSignature("numerology_calc_matrix(dob: timestamp): string")]
    public static VariantValue CalcMatrixFunction(IExecutionThread thread)
    {
        var dob = thread.Stack.Pop().AsTimestamp;
        var dateArr = NumberStringToArray(dob.ToString("dMyyyy"));

        var addNumber1 = dateArr.Sum();
        var addNumber2 = SumUntilOneDigit(addNumber1, elevenRule: true);
        var addNumber3 = addNumber1 - (2 * dateArr[0]);
        var addNumber4 = SumUntilOneDigit(addNumber3);

        var allNumbers = dateArr
            .Concat(NumberToArray(addNumber1))
            .Concat(NumberToArray(addNumber2))
            .Concat(NumberToArray(addNumber3))
            .Concat(NumberToArray(addNumber4))
            .Where(x => x > 0)
            .Order()
            .ToArray();

        var result = $"{addNumber2}," + string.Join(
            ' ',
            allNumbers.GroupBy(x => x).Select(x => string.Join(null, x))
        );

        return new VariantValue(result);
    }

    private static int SumUntilOneDigit(int num, bool elevenRule = false)
    {
        var currentNum = num;
        while (NumberToArray(currentNum).Length > 1)
        {
            if (elevenRule && currentNum == 11)
            {
                break;
            }
            currentNum = NumberToArray(currentNum).Sum();
        }
        return currentNum;
    }

    private static int[] NumberStringToArray(string str, bool removeZeros = true)
        => str.ToCharArray()
            .Where(char.IsDigit)
            .Select(c => int.Parse(c.ToString()))
            .Where(c => removeZeros || c > 0)
            .ToArray();

    private static int[] NumberToArray(int num, bool removeZeros = true)
        => NumberStringToArray(num.ToString(), removeZeros);
}
