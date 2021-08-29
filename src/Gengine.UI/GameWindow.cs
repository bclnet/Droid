using static Gengine.Library.Lib;

namespace Gengine.UI
{
    public class GameWindowProxy : Window
    {
        public GameWindowProxy(DeviceContext dc, UserInterfaceLocal gui) : base(dc, gui) { }

        public override void Draw(int time, float x, float y) =>
            common.Printf("TODO: GameWindowProxy::Draw\n");
    }
}