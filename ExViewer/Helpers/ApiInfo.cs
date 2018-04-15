using static Windows.Foundation.Metadata.ApiInformation;

namespace ExViewer
{
    public static class ApiInfo
    {
        public static bool RS3 { get; } = IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5);

        public static bool StatusBarSupported { get; } = IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static bool ShareProviderSupported { get; } = IsTypePresent("Windows.ApplicationModel.DataTransfer.ShareProvider");
    }
}
