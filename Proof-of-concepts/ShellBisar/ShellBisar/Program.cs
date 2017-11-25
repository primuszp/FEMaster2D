using System;
using System.IO;
using ShellBisar.Bisar;
using ShellBisar.Pavement;
using ShellBisar.Results;

namespace ShellBisar
{
    class MainClass
    {
        private static BisarDocument doc = new BisarDocument();

        public static void Main(string[] args)
        {
            //doc.Read(new FileStream("out.txt", FileMode.Open));

            BisarSystem bsystem = new BisarSystem();
            bsystem.Layers.Add(new Layer(1, new Material(6000, 0.35), 0.15, 1.0));
            bsystem.Layers.Add(new Layer(2, new Material(2000, 0.25), 0.25, 1.0));
            bsystem.Layers.Add(new Layer(3, new Material(50, 0.50)));

            bsystem.Loads.Add(new Load(1, 0, 0, 50.0, 0.15, 10, 15));

            PointResult pr1 = new PointResult(1, 1, 0.00);
            PointResult pr2 = new PointResult(2, 1, 0.15);
            PointResult pr3 = new PointResult(3, 3, 0.40);

            bsystem.Evaluations.Add(pr1);
            bsystem.Evaluations.Add(pr2);
            bsystem.Evaluations.Add(pr3);

            doc.Systems.Add(bsystem);
            doc.Title = "Demo Title";

            doc.Write(new FileStream("input.txt", FileMode.Create));
        }
    }
}