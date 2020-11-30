namespace EasyKeys.Extensions.Shipping.Address
{
    public interface IAddressParser
    {
        bool TryParseAddress(string input, out AddressParseResult? result);
    }
}
