using MerchantOS.Admin.Models;
using Xunit;
public sealed class ManagementTests {
 [Theory][InlineData("0")][InlineData("-1")][InlineData("1000000.01")][InlineData("NaN")][InlineData("1e3")]public void RejectUnsafePrice(string value)=>Assert.Null(new ProductEdit{Price=value}.Halalas());
 [Theory][InlineData("0.01",1)][InlineData("123.45",12345)][InlineData("1000000.00",100000000)]public void ConvertDecimalPriceExactly(string value,long expected)=>Assert.Equal(expected,new ProductEdit{Price=value}.Halalas());
}
