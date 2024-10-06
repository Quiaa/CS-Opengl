
namespace OpenGLWork
{
    class Program
    {
        static void Main(string[] args)
        {
            using(Window window = new Window(800, 800))
            {
                window.Run();
            }
            //using (Masterpiece mp = new Masterpiece(800, 800))
            //{
            //    mp.Run();
            //}
        }
    }
}