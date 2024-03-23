using System.Windows.Input;

namespace Universities.Handlers
{
    public static class KeyPressHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Numbers(Key key)
        {
            return (key >= (Key)34 && key <= (Key)43) || (key >= (Key)74 && key <= (Key)83);
        }
    }
}