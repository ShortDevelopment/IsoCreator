using Windows.UI.Xaml;

namespace IsoCreator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            FullTrustApplication.Start(_ => new App(), new("Iso Creator") { HasTransparentBackground = false });
        }
    }
}
