using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace WPF_VRP_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public class smallVeh
    {
        public string name { get; set; }
        public double maxWeight = 8000;
        public double maxLength = 7.8;
        public double actWeight { get; set; } = 0;
        public double actLength { get; set; } = 0;
    }

    public class VRPcls
    {
        public ObservableCollection<smallVeh> smlVhList { get; set; }
        public string citiesFile = null;
        public string dataFile = null;

        public VRPcls()
        {
            smlVhList = new ObservableCollection<smallVeh>();
        }
        public void vehAdd()
        {
            smallVeh smV = new smallVeh();
            smV.name = "Vehicle " + smlVhList.Count().ToString();
            smlVhList.Add(smV);
        }
        public Stack VRP(List<List<double>> citiesTab, List<List<double>> data, int hub, int citiesNum)
        {
            smallVeh actVeh = new smallVeh();
            try
            {
                actVeh = smlVhList.First(); ///aktualny pojazd 
            }
            catch (Exception e)
            {
                MessageBox.Show("There aren't any trucks to use.");
                return default;
            }

            double maxDist = 1;
            double minDist = Int32.MaxValue; 
            Stack perm = new Stack();   //permutacja odwiedzonych miast

            int k = 0;  //pojazdy
            bool limit = true;
            int indx = 0;
            int minDistIndx = 0;

            double vehWeig = actVeh.maxWeight;    //ladownosc aktualnego typu pojazdu
            double vehLeng = actVeh.maxLength;

            perm.Push(hub);

            for (int b = 0; b < citiesNum; b++)
                while (maxDist != 0)
                {
                    maxDist = 0;
                    for (int i = 0; i < citiesNum; i++)
                    {

                        if (maxDist < citiesTab[hub][i])    //wybieramy najbardziej oddalone miasto od huba
                        {
                            maxDist = citiesTab[hub][i];
                            indx = i;
                        }
                    }

                    if (maxDist != 0)   //jezeli maxDist = 0 to oznacza to, ze skonczyly sie miasta w zbiorze dostepnych miast
                    {
                        perm.Push(indx);
                        citiesTab[hub][indx] = 0;
                        citiesTab[indx][hub] = 0;
                        actVeh.actLength = data[indx][1];
                        actVeh.actWeight = data[indx][2];

                        while (limit == true)    //wybieramy miasta najblizej poprzednio wybranego miasta i wliczamy je jezeli mozemy pomiescic towar
                        {
                            for (int i = 0; i < citiesNum; i++)
                            {
                                if (minDist > citiesTab[i][indx] && citiesTab[i][hub] != 0)
                                {
                                    minDist = citiesTab[i][indx];
                                    minDistIndx = i;
                                }
                            }
                            minDist = Int32.MaxValue;

                            if ((actVeh.actLength + data[minDistIndx][1]) <= vehLeng && (actVeh.actWeight + data[minDistIndx][2]) <= vehWeig && citiesTab[minDistIndx][hub] != 0)   //jezeli towar spelnia wymogi i miasto nie zostalo wczesniej odwiedzone to jest wrzucane do permutacji
                            {
                                perm.Push(minDistIndx);
                                citiesTab[minDistIndx][hub] = 0;
                                citiesTab[hub][minDistIndx] = 0;
                                actVeh.actLength += data[minDistIndx][1];
                                actVeh.actWeight += data[minDistIndx][2];
                            }
                            else
                            {
                                //Uwzglednienie mozliwosci odwiedzenia innych miast, ktorych towar zmiesci sie jeszcze do pojazdu

                                while (limit == true)
                                {
                                    for (int i = 0; i < citiesNum; i++)
                                    {
                                        if ((actVeh.actLength + data[i][1]) <= vehLeng && (actVeh.actWeight + data[i][2]) <= vehWeig && citiesTab[i][hub] != 0 && minDist > citiesTab[minDistIndx][i] && citiesTab[minDistIndx][i] < citiesTab[minDistIndx][hub])
                                        {
                                            minDistIndx = i;
                                            minDist = citiesTab[i][hub];
                                        }
                                    }
                                    if (minDist == Int32.MaxValue)
                                    {
                                        limit = false;
                                    }
                                    else
                                    {
                                        perm.Push(minDistIndx);
                                        actVeh.actLength += data[minDistIndx][1];
                                        actVeh.actWeight += data[minDistIndx][2];
                                        citiesTab[minDistIndx][hub] = 0;
                                        minDist = Int32.MaxValue;
                                    }
                                }

                                limit = false;
                            }
                        }

                        minDist = Int32.MaxValue;
                        k++;    //jezeli pojazd k nie moze pomiescic wiecej towaru, wybieramy kolejny pojazd
                        try
                        {
                            actVeh = smlVhList.ElementAt<smallVeh>(k);
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show("There aren't enough trucks.");
                            return default;
                        }

                        limit = true;
                        perm.Push(hub);
                    }
                }
            return perm;
        }

        static List<List<double>> readFileFunc(string fileName, int startLine)  //funkcja do sczytywania plikow excelowych
        {
            List<string> fileHelper = new List<string>();
            List<List<double>> data = new List<List<double>>();
            string line;
            String[] citiesS;
            int citiesNumb;
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);


            fileHelper.Clear();
            data.Clear();
            while ((line = file.ReadLine()) != null)
            {
                fileHelper.Add(line);
            }

            citiesS = fileHelper[0].Split(';');
            citiesNumb = Convert.ToInt32(citiesS[0]);

            if (startLine != 27)
            {
                for (int i = startLine; i < citiesNumb + startLine; i++)
                {
                    List<double> pom = new List<double>();
                    String[] elements = fileHelper[i].Split(';');
                    foreach (var element in elements)
                    {
                        pom.Add(Convert.ToDouble(element));
                    }
                    data.Add(pom);
                }
            }
            else if (startLine == 27)   //sczytywanie wspolrzednych miast
            {
                for (int i = 27; i < citiesNumb + 27; i++)
                {
                    List<double> pom = new List<double>();
                    String[] elements = fileHelper[i].Split(';');
                    for (int j = 2; j < 4; j++)
                    {
                        pom.Add(Convert.ToDouble(elements[j]));
                    }
                    data.Add(pom);
                }
            }
            file.Close();

            return data;
        }

        static double wholeDist(Stack perm, List<List<double>> Odleglosci, int hub) //obliczanie calkowitego przejechanego dystansu
        {
            int prevCity = hub;
            double wDist = 0;
            int elemTemp = 0;
            foreach (int elem in perm)
            {
                elemTemp = elem;

                wDist += Odleglosci[prevCity][elemTemp];
                prevCity = elemTemp;
            }
            return wDist;
        }

        public double[] VRPmain()
        {
            List<string> Plik = new List<string>();
            List<List<double>> Odleglosci = new List<List<double>>();
            List<List<double>> data = new List<List<double>>();
            List<List<double>> grids = new List<List<double>>();

            Stack perm = new Stack();
            string line;
            int rozmiar;
            int hub = 0;
            double wholeDistance = 0;

            double bestWholeDistance = Int32.MaxValue;
            int besthub = 0;
            Stack bestPerm = new Stack();

            System.IO.StreamReader file = new System.IO.StreamReader(citiesFile);
            line = file.ReadLine();
            Plik.Add(line);
            String[] rozmiar_st = Plik[0].Split(';');
            rozmiar = Convert.ToInt32(rozmiar_st[0]);
            file.Close();

            data = readFileFunc(dataFile, 1);
            Odleglosci = readFileFunc(citiesFile, 2);
            perm = VRP(Odleglosci, data, hub, rozmiar);
            if (perm == default) return default;
            Odleglosci = readFileFunc(citiesFile, 2);
            grids = readFileFunc(citiesFile, 27);
            wholeDistance = wholeDist(perm, Odleglosci, hub);

            for (int i = 0; i < rozmiar; i++)    //wybieramy miasto, w którym najlepiej jest ustalic huba
            {
                perm = VRP(Odleglosci, data, i, rozmiar);
                if (perm == default) return default;
                Odleglosci = readFileFunc(citiesFile, 2);
                wholeDistance = wholeDist(perm, Odleglosci, i);
                if (wholeDistance < bestWholeDistance)
                {
                    bestWholeDistance = wholeDistance;
                    besthub = i;
                    bestPerm = perm;
                }
            }

            double[] outTab = new double[2];
            outTab[0] = besthub;
            outTab[1] = bestWholeDistance;      
            return outTab;
        }


    }

    public partial class MainWindow : Window
    {
        public VRPcls newOne;

        public MainWindow()
        {
            newOne = new VRPcls();
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            newOne.vehAdd();
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
           WholeCntnt.Text = newOne.smlVhList.Count.ToString();
        }

        private void VRPexec_Click(object sender, RoutedEventArgs e)
        {
            if(newOne.citiesFile == null || newOne.dataFile == null)
            {
                MessageBox.Show("First choose cities and data files");
                return;
            }
            string dist = null;
            double[] outTab = newOne.VRPmain();
            if (outTab == default) {
                MessageBox.Show("Can't calculate the solution.");
                return;
            }
            dist = outTab[1].ToString();
            dist = dist.Substring(0, dist.LastIndexOf(",")+3);
            WholeCntnt.Text = "Best hub is: " + outTab[0].ToString() + ", whole distance is: " + dist + "km";
            VchList.DataContext = newOne;
        }

        private void bt1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opnFile = new OpenFileDialog();
            if (opnFile.ShowDialog() == true)
            {
                newOne.citiesFile = opnFile.FileName;
            }
        }

        private void bt2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opnFile = new OpenFileDialog();
            if (opnFile.ShowDialog() == true)
            {
                newOne.dataFile = opnFile.FileName;
            }
        }
    }
}
