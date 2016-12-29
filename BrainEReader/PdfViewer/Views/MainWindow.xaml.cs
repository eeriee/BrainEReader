using System.IO;
using System.Windows;
//using Microsoft.Win32;
using System.Threading;
using System.Windows.Media;
using NeuroSky.ThinkGear;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Forms;
using System.Reflection;
using System.Media;
using System.Windows.Threading;
using System.Data;
//using System.Numerics;
using MathNet.Numerics;
using Accord;
using Accord.Controls;
using Accord.IO;
using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Statistics.Kernels;
using Accord.Statistics.Models.Regression.Linear;
using AForge;
using System.Linq;
//using libsvm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsInput;
using WindowsInput.Native;
using Microsoft.Practices.Unity;
using EEGPdfViewer.Services;
using EEGPdfViewer.Infrastructure;
using EEGPdfViewer.ViewModel;
using EEGPdfViewer.EEGModel;
using System.Diagnostics;
//using System.Drawing;
//using WindowsInput;
//using WindowsInput.Native;
//using alglib;
//using AForge.Math;
//using CsvHelper;

namespace EEGPdfViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window, IMainWindow
    {

        //private C_SVC model;
        private MulticlassSupportVectorMachine machine;
        private MulticlassSupportVectorLearning teacher;
        private KNearestNeighbors knn;
        //private SupportVectorMachine machine;
        //private SequentialMinimalOptimization teacher;
        private Connector connector;
        private byte poorSig;
        private Queue<int> predCommand;
        private int blinkSum, blinkFre, loop, cloop, close, testingIndex, command;
        private double rawdata, delta, theta, alpha1, alpha2, beta1, beta2, gamma1, gamma2, att, med;
        private List<double> eeg_power, raw_values0, raw_values1;
        private double[][] testingData;
        private bool prev, next, exp, startCommand, countBlink;
        private string text, blinktxt, confirmtext;
        private string label = "2";
        private static string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
        private static string samplePdfPath = Path.Combine(Path.GetDirectoryName(location), "sample.pdf");
        private static SoundPlayer closeplayer = new SoundPlayer(@"Audio\close.wav");
        private static SoundPlayer nextplayer = new SoundPlayer(@"Audio\next.wav");
        private static SoundPlayer prevplayer = new SoundPlayer(@"Audio\prev.wav");
        private static SoundPlayer nextConfirmPlayer = new SoundPlayer(@"Audio\nextConfirm.wav");
        private static SoundPlayer prevConfirmPlayer = new SoundPlayer(@"Audio\prevConfirm.wav");
        private static int sampleSize = 300;
        private static int psNumber = 4;
        private static int confidenceIntervalParameter = 3;
        private static int startBins = 0;
        private static int endBins = 60; //<=256
        private static double comp = 10;
        private static string kernelName = "linear";
        private static IKernel kernel = new Linear();
        private static string method = "avg";
        private static string modelName = "svm";
        private static int classNum = 2;
        private string folderName = "training_data";
        private static int page = 1;
        private static int partition = 1;
        private static int start;
        private static Stopwatch stopwatch = new Stopwatch();
        private static MainWindowViewModel mainWindowView;
        private static PopupWindowViewModel popupView = new PopupWindowViewModel();
        private static readonly IUnityContainer _container = UnityContainerResolver.Container;
        private static IChildWindow childWindow = _container.Resolve<IChildWindow>();
        private static int knear = 5;

        public MainWindow()
        {
            InitializeComponent();
            init();
            Closing += MainWindowClosing;
            mainWindowView = new MainWindowViewModel(this, childWindow);
            DataContext = mainWindowView;
            Signal.DataContext = Status.DataContext = mainWindowView;
            Confirm.DataContext = Timer.DataContext = Popup.DataContext = PopupInfo.DataContext = popupView;
            Start_Button.DataContext = Stop_Button.DataContext = childWindow;
            //closeplayer.Load();
            nextplayer.Load();
            nextConfirmPlayer.Load();
            prevplayer.Load();
            prevConfirmPlayer.Load();
            //load pdf
            pdfViewer.LoadFile(samplePdfPath, page); //path, page
            //pdfToFullScreen();
            connector = new Connector();
            connector.DeviceConnected += new EventHandler(OnDeviceConnected);
            connector.DeviceConnectFail += new EventHandler(OnDeviceNotFound);
            connector.DeviceNotFound += new EventHandler(OnDeviceNotFound);
            connector.DeviceDisconnected += new EventHandler(OnDeviceDisconnected);
            connector.DeviceValidating += new EventHandler(OnDeviceValidating);

            // Scan for devices across COM ports
            // The COM port named will be the first COM port that is checked.
            connector.ConnectScan("COM3");

            // Blink detection needs to be manually turned on
            connector.setBlinkDetectionEnabled(true);


            

        }
        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            disconnectEEG();
            Environment.Exit(0);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            
            showTotalPage.DataContext = pdfViewer;
            showCurrentPage.DataContext = pdfViewer;
            pdfToFullScreen();
            if (childWindow.getModeIsChecked())
            {
                //training data pre-processing
                double[][] training = new double[sampleSize * partition * classNum][];
                int index = 0;
                int startI = 0;

                if (classNum == 2) startI = 1;
                for (int i = startI; i < 3; i++)
                {
                    for (int j = 0; j < sampleSize; j++)
                    {

                        string json = File.ReadAllText("exp\\" + folderName +"\\" + i + "\\" + (j) + ".json");
                        EEGSample sample = JsonConvert.DeserializeObject<EEGSample>(json);
                        if (partition == 1)
                        {
                            training[index++] = sample.Raw_Values;
                        }
                        else
                        {
                            for (int k = 0; k < partition; k++)
                            {
                                int len = sample.Raw_Values.Length / partition;
                                double[] tmp = new double[len];
                                for (int h = 0; h < len; h++)
                                {
                                    tmp[h] = sample.Raw_Values[h];
                                }
                                training[index++] = (double[])tmp.Clone();
                            }
                        }

                    }
                }
                double[][] psTraining = new double[index][];
                for (int i = 0; i < index; i++)
                {
                    psTraining[i] = powerSpectrum(training[i]);
                }
                double[][] inputs = new double[index / psNumber][];

                for (int i = 0, j = 0; i < index; i += psNumber, j++)
                {
                    double[][] multi_pspectra = new double[psNumber][];
                    for (int k = 0; k < psNumber; k++)
                    {
                        multi_pspectra[k] = psTraining[i + k];
                    }

                    if (method == "percent")
                        inputs[j] = percentilePowerSpectrum(powerSpectraArray(multi_pspectra, endBins), confidenceIntervalParameter);
                    else if (method == "avg")
                        inputs[j] = avgPowerSpectrum(powerSpectraArray(multi_pspectra, endBins));
                }

                int[] outputs = new int[inputs.GetLength(0)];

                int ptr = 0;

                for (int i = 0; i < classNum; i++)
                {
                    for (int j = 0; j < index / classNum; j += psNumber)
                    {
                        outputs[ptr++] = i;

                    }
                }

                string modelPath = "exp\\" + folderName + "\\training\\" + classNum + "_" + method + "_" + modelName
                    + "_" + kernelName + "_" + psNumber + "_" + sampleSize + "_" + partition + "_" + startBins + "-" + endBins + "_" + knear + "_" + comp + ".txt";

                //training model
                if (File.Exists(modelPath))
                {
                    trainModel(inputs, outputs, modelPath);
                }
                else
                {
                    validateModel(inputs, outputs, modelPath);
                }
            }

            

        }

        public void validateModel(double[][] inputs, int[] outputs, string modelPath)
        {
            string resultPath = "exp\\" + folderName + "\\training\\result_" + classNum + "_" + method + "_" + modelName
                + "_" + kernelName + "_" + psNumber + "_" + sampleSize + "_" + partition + "_" + startBins + "-" + endBins + "_" + knear + "_" + comp + ".txt";

            //cross-validation
            var crossvalidation = new CrossValidation(size: inputs.Length, folds: 5);

            string[] answerArr, outputsArr;
            int[] answers = new int[outputs.Length];
            double trainingAcc, validationAcc, accuracy;
            trainingAcc = validationAcc = accuracy = 0;

            if (modelName == "svm")
            {

                crossvalidation.Fitting = delegate(int k, int[] indicesTrain, int[] indicesValidation)
                {
                    var trainingInputs = inputs.Submatrix(indicesTrain);
                    var trainingOutputs = outputs.Submatrix(indicesTrain);

                    var validationInputs = inputs.Submatrix(indicesValidation);
                    var validationOutputs = outputs.Submatrix(indicesValidation);

                    var multiSVM = new MulticlassSupportVectorMachine(endBins, kernel, classNum);
                    var smo = new MulticlassSupportVectorLearning(multiSVM, trainingInputs, trainingOutputs);

                    smo.Algorithm = (svm, classInputs, classOutputs, i, j) =>
                            new SequentialMinimalOptimization(svm, classInputs, classOutputs)
                            {
                                Complexity = comp
                            };
                    double trainingError = smo.Run();
                    double validationError = smo.ComputeError(validationInputs, validationOutputs);
                    return new CrossValidationValues(multiSVM, trainingError, validationError);
                };

                var result = crossvalidation.Compute();

                trainingAcc = 1 - result.Training.Mean;
                validationAcc = 1 - result.Validation.Mean;



                machine = new MulticlassSupportVectorMachine(endBins, kernel, classNum);
                Directory.CreateDirectory("exp\\" + folderName + "\\training");
                machine.Save(modelPath);
                teacher = new MulticlassSupportVectorLearning(machine, inputs, outputs);

                teacher.Algorithm = (svm, classInputs, classOutputs, i, j) =>
                        new SequentialMinimalOptimization(svm, classInputs, classOutputs)
                        {
                            Complexity = comp
                        };

                accuracy = 1 - teacher.Run();
                answers = inputs.Apply(machine.Compute);
            }
            else if (modelName == "knn")
            {

                crossvalidation.Fitting = delegate(int k, int[] indicesTrain, int[] indicesValidation)
                {
                    // The fitting function is passing the indices of the original set which
                    // should be considered training data and the indices of the original set
                    // which should be considered validation data.

                    // Lets now grab the training data:
                    var trainingInputs = inputs.Submatrix(indicesTrain);
                    var trainingOutputs = outputs.Submatrix(indicesTrain);

                    // And now the validation data:
                    var validationInputs = inputs.Submatrix(indicesValidation);
                    var validationOutputs = outputs.Submatrix(indicesValidation);

                    var vknn = new KNearestNeighbors(k: knear, classes: classNum,
                        inputs: trainingInputs, outputs: trainingOutputs);

                    int[] train_predicted = trainingInputs.Apply(vknn.Compute);
                    int[] test_predicted = validationInputs.Apply(vknn.Compute);

                    // Compute classification error
                    var cmTrain = new ConfusionMatrix(train_predicted, trainingOutputs);
                    double trainingAccs = cmTrain.Accuracy;

                    // Compute the validation error on the validation data:
                    var cmTest = new ConfusionMatrix(test_predicted, validationOutputs);
                    double validationAccs = cmTest.Accuracy;

                    // Return a new information structure containing the model and the errors achieved.
                    return new CrossValidationValues(knn, trainingAccs, validationAccs);
                };


                // Compute the cross-validation
                var result = crossvalidation.Compute();

                // Finally, access the measured performance.
                trainingAcc = result.Training.Mean;
                validationAcc = result.Validation.Mean;

                knn = new KNearestNeighbors(k: knear, classes: classNum,
                        inputs: inputs, outputs: outputs);

                answers = inputs.Apply(knn.Compute);
                var totalTrain = new ConfusionMatrix(answers, outputs);
                accuracy = totalTrain.Accuracy;

            }

            Console.WriteLine(trainingAcc + "  " + validationAcc);
            answerArr = answers.Select(x => x.ToString()).ToArray();
            outputsArr = outputs.Select(x => x.ToString()).ToArray();

            if (File.Exists(resultPath))
            {
                using (StreamWriter sw = File.AppendText(resultPath))
                {
                    sw.WriteLine(string.Join(" ", answerArr));
                    sw.WriteLine(string.Join(" ", outputsArr));
                    sw.WriteLine("trainMean:" + trainingAcc + " validationMean:" + validationAcc + " total:" + accuracy);
                }
            }
            else
            {
                using (StreamWriter sw = File.CreateText(resultPath))
                {
                    sw.WriteLine(string.Join(" ", answerArr));
                    sw.WriteLine(string.Join(" ", outputsArr));
                    sw.WriteLine("trainMean:" + trainingAcc + " validationMean:" + validationAcc + " total:" + accuracy);
                }
            }

        }

        public void trainModel(double[][] inputs, int[] outputs, string modelPath)
        {
            if (modelName == "svm")
            {
                machine = MulticlassSupportVectorMachine.Load(modelPath);
                teacher = new MulticlassSupportVectorLearning(machine, inputs, outputs);
                teacher.Algorithm = (svm, classInputs, classOutputs, i, j) =>
                    new SequentialMinimalOptimization(svm, classInputs, classOutputs)
                    {
                        Complexity = comp
                    };

                double error = teacher.Run();
            }
            else if (modelName == "knn")
            {
                knn = new KNearestNeighbors(k: knear, classes: classNum,
                        inputs: inputs, outputs: outputs);
            }

        }
        public double[][] getTransformArrary(double[][] arr)
        {
            int row = arr[0].Length;
            int col = arr.GetLength(0);
            double[][] tArr = new double[row][];
            for (int i = 0; i < row; i++)
            {
                double[] tmp = new double[col];
                for (int j = 0; j < col; j++)
                {
                    tmp[j] = arr[j][i];
                }
                tArr[i] = tmp;
            }
            return tArr;
        }
        public double[] powerSpectrum(double[] data)
        {
            //LomontFFT.getInstance().FFT(data, true);
            alglib.complex[] f;
            alglib.fftr1d(data, out f);

            double[] tmp = new double[f.Length / 2];
            for (int i = 0; i < f.Length / 2; i++)
            {
                tmp[i] = Math.Pow(alglib.math.abscomplex(f[i]), 2);
                //tmp[i] = alglib.math.abscomplex(f[i]);
            }
            return tmp;

        }

        public double[][] powerSpectraArray(double[][] pspectra, int nbins)
        {
            //int psNumber = pspectra.GetLength(0);
            int psLength = pspectra[0].Length;
            double[][] arr = new double[psNumber][];
            double[] sum = new double[psNumber];
            Array.Clear(sum, 0, psNumber);
            double[] x = new double[psLength];
            for (int i = 0; i < psLength; i++)
            {
                x[i] = i + 1;
            }

            /* for (int i = 0; i < psNumber; i++)
             {
                 for (int j = 0; j < psLength; j++)
                 {
                     sum[i] += pspectra[i][j];
                 }
             }
             for (int i = 0; i < psNumber; i++)
             {
                 for (int j = 0; j < pspectra[0].Length; j++)
                 {
                     pspectra[i][j] = pspectra[i][j] / sum[i];
                 }
             } */
            for (int i = 0; i < psNumber; i++)
            {
                var f = Interpolate.Linear(x, pspectra[i]);
                List<double> tmpX = new List<double>();
                for (int j = 0; j < nbins; j++)
                {
                    tmpX.Add(f.Interpolate(j + 1));
                }
                arr[i] = tmpX.ToArray();
            }
            /*using (StreamWriter sw = File.AppendText("C:\\Users\\WANG XI\\psArrdata.txt"))
            {
                sw.WriteLine(string.Join(" ", arr[0].Select(y => y.ToString()).ToArray()));
            } */
            return arr;
        }

        public double percentile(double[] sequence, double percent)
        {
            //percentile function testing
            /* using (System.IO.StreamWriter sw = new System.IO.StreamWriter("C:\\Users\\WANG XI\\testpercent.txt"))
             {
                 sw.WriteLine(percentile(new double[] { 1, 2, 3, 4, 7, 10 }, 50).ToString());
                 sw.WriteLine(percentile(new double[] { 1, 2, 3, 4, 7, 10 }, 100).ToString());
                 sw.WriteLine(percentile(new double[] { 1, 2, 3, 4, 7, 10 }, 0).ToString());
                 sw.WriteLine(percentile(new double[] { 1, 4, 7, 10 }, 80).ToString());
             } */
            Array.Sort(sequence);
            int N = sequence.Length;
            double n = (N - 1) * (percent / 100) + 1;
            if (n == 1) return sequence[0];
            else if (n == N) return sequence[N - 1];
            else
            {
                int k = (int)n;
                double d = n - k;
                return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
            }
        }

        public double[] percentilePowerSpectrum(double[][] arrayOfPowerSpectra, double percent) //avgPercentileUp
        {
            int len = arrayOfPowerSpectra[0].Length;
            double[] arr = new double[len];
            double per;
            double[][] tArrayofPowerSpectra = getTransformArrary(arrayOfPowerSpectra);
            for (int i = 0; i < len; i++)
            {
                per = percentile(tArrayofPowerSpectra[i], 100 - percent);
                arr[i] = per;
            }
            return arr;
        }

        public double[] avgPowerSpectrum(double[][] arrayOfPowerSpectra)
        {
            int len = arrayOfPowerSpectra[0].Length;
            double[] arr = new double[len];
            double avg;
            double[][] tArrayofPowerSpectra = getTransformArrary(arrayOfPowerSpectra);
            for (int i = 0; i < len; i++)
            {
                avg = tArrayofPowerSpectra[i].Average();
                arr[i] = Math.Log10(avg);
                // Console.WriteLine(arr[i]);
            }

           /* using (StreamWriter sw = File.AppendText("C:\\Users\\WANG XI\\avgpdata.txt"))
            {
                sw.WriteLine(string.Join(" ", arr.Select(x => x.ToString()).ToArray()));

            } */
            return arr;
        }

        public void init()
        {
            blinkSum = blinkFre = loop = cloop = close = 0;
            delta = theta = alpha1 = alpha2 = gamma1 = gamma2 = beta1 = beta2 = 0;
            prev = next = exp = startCommand = false;
            countBlink = true;
            raw_values0 = new List<double>();
            raw_values1 = new List<double>();
            eeg_power = new List<double>();
            testingData = new double[psNumber][];
            predCommand = new Queue<int>();
            testingIndex = command = 0;
            confirmtext = "";
            popupView.ConfirmIsOpen = popupView.InfoIsOpen = false;
        }

        public void OnDeviceConnected(object sender, EventArgs e)
        {

            Connector.DeviceEventArgs de = (Connector.DeviceEventArgs)e;
            mainWindowView.HeadsetStatus = "Connected!";
            init();
            de.Device.DataReceived += new EventHandler(OnDataReceived);
        }

        public void OnDeviceDisconnected(object sender, EventArgs e)
        {
            mainWindowView.HeadsetStatus = "Disconnected!";
            mainWindowView.Signal = "No Signal!";
            popupView.InfoIsOpen = false;
            popupView.ConfirmIsOpen = false;
            connector.ConnectScan("COM3"); 
        }

        // Called when each port is being validated

        public void OnDeviceNotFound(object sender, EventArgs e)
        {
            mainWindowView.HeadsetStatus = "Disconnected!";
            mainWindowView.Signal = "No Signal!";
            popupView.InfoIsOpen = false;
            popupView.ConfirmIsOpen = false;
            connector.ConnectScan("COM3");

        }

        public void OnDeviceValidating(object sender, EventArgs e)
        {
            mainWindowView.HeadsetStatus = "Disconnected!";
            mainWindowView.Signal = "No Signal!";
            popupView.InfoIsOpen = false;
            popupView.ConfirmIsOpen = false;
        }

        public void OnDataReceived(object sender, EventArgs e)
        {
            //Device d = (Device)sender;
            Device.DataEventArgs de = (Device.DataEventArgs)e;
            //NeuroSky.ThinkGear.DataRow
            NeuroSky.ThinkGear.DataRow[] tempDataRowArray = de.DataRowArray;

            TGParser tgParser = new TGParser();
            if (childWindow.IsDisplay()) init();
            else tgParser.Read(de.DataRowArray);
            /* Loops through the newly parsed data of the connected headset*/
            // The comments below indicate and can be used to print out the different data outputs. 

            for (int i = 0; i < tgParser.ParsedData.Length; i++)
            {


                if (tgParser.ParsedData[i].ContainsKey("PoorSignal"))
                {

                    //The following line prints the Time associated with the parsed data
                    //Console.WriteLine("Time:" + tgParser.ParsedData[i]["Time"]);

                    //A Poor Signal value of 0 indicates that your headset is fitting properly
                    //Console.WriteLine("Poor Signal:" + tgParser.ParsedData[i]["PoorSignal"]);

                    poorSig = (byte)tgParser.ParsedData[i]["PoorSignal"];
                    if (poorSig < 51)
                    {
                        mainWindowView.Signal = "Good!";
                        popupView.InfoIsOpen = false;
                    }
                    else
                    {
                        init();
                        popupView.InfoIsOpen = true;
                        popupView.ConfirmIsOpen = false;
                        mainWindowView.Signal = "Poor!";
                    }
                    //text += String.Format("{0}\t ", poorSig);
                }
                // if (poorSig < 51)
                //  {
                //if (startCommand) 
                if (poorSig < 51)
                {
                    if (startCommand && !next && !prev) 
                    {
                        // Console.WriteLine("" + tgParser.ParsedData[i]["Time"]);

                        if (tgParser.ParsedData[i].ContainsKey("Raw"))
                        {

                            //++cloop;
                            rawdata = tgParser.ParsedData[i]["Raw"];
                            if (raw_values0.Count < 256)
                                raw_values0.Add(rawdata);
                            else
                                raw_values1.Add(rawdata);
                            //text += String.Format("{0}\t", cloop);
                            //text += String.Format("{0}\t ", tgParser.ParsedData[i]["Raw"]);
                            //Console.WriteLine("Raw Value:" + tgParser.ParsedData[i]["Raw"]);

                        }

                        if (tgParser.ParsedData[i].ContainsKey("EegPowerDelta"))
                        {
                            //++cloop;

                            delta = tgParser.ParsedData[i]["EegPowerDelta"];
                            eeg_power.Add(delta);
                            text += String.Format("{0}\t", cloop);
                            text += String.Format("{0}\t", delta);
                        }
                        if (tgParser.ParsedData[i].ContainsKey("EegPowerTheta"))
                        {
                            theta = tgParser.ParsedData[i]["EegPowerTheta"];
                            eeg_power.Add(theta);
                            text += String.Format("{0}\t", theta);
                        }
                        if (tgParser.ParsedData[i].ContainsKey("EegPowerAlpha1"))
                        {

                            alpha1 = tgParser.ParsedData[i]["EegPowerAlpha1"];
                            eeg_power.Add(alpha1);
                            text += String.Format("{0}\t", alpha1);
                        }
                        if (tgParser.ParsedData[i].ContainsKey("EegPowerAlpha2"))
                        {

                            alpha2 = tgParser.ParsedData[i]["EegPowerAlpha2"];
                            eeg_power.Add(alpha2);
                            text += String.Format("{0}\t", alpha2);
                        }
                        if (tgParser.ParsedData[i].ContainsKey("EegPowerBeta1"))
                        {

                            beta1 = tgParser.ParsedData[i]["EegPowerBeta1"];
                            eeg_power.Add(beta1);
                            text += String.Format("{0}\t", beta1);
                        }
                        if (tgParser.ParsedData[i].ContainsKey("EegPowerBeta2"))
                        {
                            beta2 = tgParser.ParsedData[i]["EegPowerBeta2"];
                            eeg_power.Add(beta2);
                            text += String.Format("{0}\t", beta2);
                        }
                        if (tgParser.ParsedData[i].ContainsKey("EegPowerGamma1"))
                        {

                            gamma1 = tgParser.ParsedData[i]["EegPowerGamma1"];
                            eeg_power.Add(gamma1);
                            text += String.Format("{0}\t", gamma1);
                        }
                        if (tgParser.ParsedData[i].ContainsKey("EegPowerGamma2"))
                        {
                            gamma2 = tgParser.ParsedData[i]["EegPowerGamma2"];
                            eeg_power.Add(gamma2);
                            text += String.Format("{0}\t", gamma2);

                        }

                        if (tgParser.ParsedData[i].ContainsKey("Attention"))
                        {
                            att = tgParser.ParsedData[i]["Attention"];
                            text += String.Format("{0}\t ", att);
                        }

                        if (tgParser.ParsedData[i].ContainsKey("Meditation"))
                        {

                            med = tgParser.ParsedData[i]["Meditation"];

                            /* if (close == 1)
                             {
                                 if (med == 100) ++close;
                                 else close = 0;
                             }
                             else if (close == 0 && med == 100)
                             {
                                 ++close;
                             } */
                            if (raw_values0.Count + raw_values1.Count == 512)
                            {
                                double[] rawArr0 = raw_values0.ToArray();
                                double[] rawArr1 = raw_values1.ToArray();
                                if (!childWindow.getModeIsChecked())
                                {
                                    writeEEGSample(rawArr0);
                                    writeEEGSample(rawArr1);
                                }
                                else
                                {
                                    testingData[testingIndex++] = powerSpectrum(rawArr0);
                                    testingData[testingIndex++] = powerSpectrum(rawArr1);
                                }
                                /* string tempPath = @"C:\Users\WANG XI\Exp\color\real_time_powerspectrum.txt";
                                 using (StreamWriter sw = File.AppendText(tempPath))
                                 {
                                     sw.WriteLine(string.Join(" ", powerSpectrum(rawArr0).Select(x => x.ToString()).ToArray()));
                                     sw.WriteLine(string.Join(" ", powerSpectrum(rawArr1).Select(x => x.ToString()).ToArray()));
                                 } */
                                ++cloop;

                            }
                            raw_values0 = new List<double>();
                            raw_values1 = new List<double>();
                            eeg_power = new List<double>();
                            text += String.Format("{0}\t", med);
                            text += String.Format("{0}\t ", label);
                            text += System.Environment.NewLine;
                            if (childWindow.getModeIsChecked() && testingIndex == psNumber)
                            {
                                double[] testingInput = new double[endBins];
                                if (method == "percent")
                                    testingInput = percentilePowerSpectrum(powerSpectraArray(testingData, endBins), confidenceIntervalParameter);
                                else if (method == "avg")
                                    testingInput = avgPowerSpectrum(powerSpectraArray(testingData, endBins));

                                /* string tempPath = @"C:\Users\WANG XI\Exp\color\real_time_testinginput.txt";
                                 using (StreamWriter sw = File.AppendText(tempPath))
                                 {
                                     sw.WriteLine(string.Join(" ", testingInput.Select(x => x.ToString()).ToArray()));
                               
                                 } */

                                int result = 0;
                                switch (modelName)
                                {
                                    case "svm":
                                        result = machine.Compute(testingInput);
                                        break;
                                    case "knn":
                                        result = knn.Compute(testingInput);
                                        break;
                                }

                                result += 3 - classNum;
                                if (childWindow.getModeIsChecked())
                                {
                                    predCommand.Enqueue(result);

                                    switch (testPredCommand())
                                    {
                                        case 1:
                                            if (childWindow.getPopupIsChecked())
                                                SetTurnToPrev();
                                            else
                                                TurnToPrev();
                                            /* ++command;
                                             command = command % 2;
                                             activateCommand(command); */
                                            //Console.Write(command);
                                            break;
                                        case 2:
                                            if (childWindow.getPopupIsChecked())
                                                SetTurnToNext();
                                            else
                                                TurnToNext();
                                            //confirmCommand(command);
                                            break;
                                        default:
                                            break;
                                    } 

                                }
                                writeTestingOutput(result.ToString());
                                testingData = new double[psNumber][];
                                testingIndex = 0;

                            }
                            if (cloop == 10 && !childWindow.getModeIsChecked())
                            {
                                exp = startCommand = false;
                                cloop = 0;
                                popupView.ConfirmIsOpen = false;
                                this.Dispatcher.Invoke((Action)(() =>
                                {
                                    typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Start_Button, new object[] { false });
                                    typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Stop_Button, new object[] { true });
                                }));
                            }

                        }
                    }

                    //}

                    /*double[][] tmp = new double[psNumber][];
                    for (int j = 0; j < psNumber; j++)
                    {
                        tmp[j] = (double[])testingData[j].Clone();
                    }*/
                    /*    if (cloop == 4)
                        {
                            exp = false;
                            string path = @"C:\Users\WANG XI\Exp2\exp_red";
                            int index = 0;
                            string f;
                            do
                            {
                                ++index;
                                f = path + index + ".txt";
                            } while (File.Exists(f));
                            System.IO.File.WriteAllText(f, text);
                        } */
                    //colorinput = label+" 1:"+ theta +" 2:"+alpha1+" 3:"+alpha2 +" 4:"+ beta1 +" 5:"+ beta2;
                    /*         colorinput = label + " 1:" + alpha1 + " 2:" + alpha2 + " 3:" + beta1 + " 4:" + beta2;
                             //coloroutput = machine.Compute(colorinput);
                             //changeSignal(coloroutput.ToString());

                             string tempPath = @"C:\Users\WANG XI\exp_color_temp.txt";
                             using (StreamWriter sw = File.CreateText(tempPath))
                             {
                                 sw.WriteLine(colorinput);
                             }
                             //var x, y, predY;
                             var test = ProblemHelper.ReadAndScaleProblem("C:\\Users\\WANG XI\\exp_color_temp.txt");
                             string path = @"C:\Users\WANG XI\exp_color_test.txt";


                             for (int j = 0; j < test.l; j++)
                             {
                                 var x = test.x[j];
                                 var y = test.y[j];
                                 var predY = model.Predict(x);
                                 changeStatus(x.ToString());
                                 changeSignal(predY.ToString());
                                 using (StreamWriter sw = File.AppendText(path))
                                 {
                                     sw.WriteLine(colorinput + " pred:" + predY);
                                 }
                             }
                              */


                    //var predictedY = model.Predict(x); // returns the predicted value for 'x' attributes
                    //var probabilities = model.PredictProbabilities(x);  // returns the probabilities for each class
                    // Note : in about accuracy% of cases, 'predictedY' should be equal to 'y'

                    //   }

                    /* if (close == 2)
                     {
                         //SetExitApplication();
                         loop = blinkFre = 0;
                     }
                     if (close == 3 && loop == 512)
                     {
                         loop = blinkFre = 0;
                         ++close;
                     }
                      */
                    if (next || prev) //|| close > 3)
                    {
                        int curTimeDuration = (int)stopwatch.ElapsedMilliseconds;
                        if (curTimeDuration < 4000)
                        {
                            popupView.TimerText = (3 - curTimeDuration / 1000).ToString();
                            if (curTimeDuration > 500)
                                countBlink = true;
                        }
                        else
                        {
                            if (next)
                            {
                                TurnToNext();
                                //next = false;
                                //changePopupStatus(false);
                            }
                            else if (prev)
                            {
                                TurnToPrev();
                                //prev = false;
                                //changePopupStatus(false);
                            }
                            /*else if (close > 3)
                            {
                                //close = 0;
                                //changePopupStatus(false);
                            } */
                            // stopwatch.Reset();
                            popupView.TimerText = "";
                            blinkFre = loop = 0;
                        }
                    }

                    if (tgParser.ParsedData[i].ContainsKey("BlinkStrength"))
                    {
                        int blink = (int)tgParser.ParsedData[i]["BlinkStrength"];
                        //string s = blink.ToString();
                        // changeStatus(s);
                        if (countBlink)
                        {
                            if (prev)
                            {
                                prev = false;
                                popupView.ConfirmIsOpen = false;
                                blinkFre = loop = 0;
                                //TurnToPrev();
                            }
                            else if (next)
                            {
                                next = false;
                                popupView.ConfirmIsOpen = false;
                                blinkFre = loop = 0;
                                //TurnToNext();
                            }
                            /* else if (close > 3)
                             {
                                 ExitApplication();
                             } */
                            else if (loop < 1024)
                            {
                                popupView.ConfirmIsOpen = false;
                                blinkSum += blink;
                                if (blink > 70)
                                    ++blinkFre;
                                if (blinkFre >= 2 && childWindow.getModeIsChecked())
                                {
                                    SetCommand();
                                }
                            }
                            else
                            {
                                blinkFre = loop = 0;
                            }

                        }

                    }
                   /* if (countBlink)
                    {
                        if (loop < 1024)
                        {
                            if (blinkFre >= 2 && childWindow.getModeIsChecked())
                            {
                                SetCommand();
                            }
                        }
                        else
                        {
                            blinkFre = loop = 0;
                        }
                    } */
    
                    ++loop;
                }
            }

        }

        public int testPredCommand()
        {
            if (predCommand.Count == 3)
            {
                int firstComm = predCommand.Dequeue();
                int secondComm = predCommand.Peek();
                if (firstComm == secondComm)
                {
                    writeTestingOutput(firstComm + "*****one command");
                    return firstComm;

                }
            }
            return 0;
        }
        public void writeTestingOutput(string output)
        {
            string pathFolder = "exp\\testing_result";
            string path = pathFolder + "\\"+ DateTime.Now.ToString("yyyyMMdd")+"_testing_log.txt";
            Directory.CreateDirectory(pathFolder);
            if (File.Exists(path))
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + label + " " + output);
                }
            }
            else
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + label + " " + output);
                }
            }
        }
        public void writeEEGSample(double[] rawdata)
        {
            EEGSample sample = new EEGSample
            {
                Label = label,
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Raw_Values = rawdata,
                EEG_Power = eeg_power.ToArray(),
                Attention = att,
                Meditation = med,
                Signal_Quality = poorSig
            };

            string _task = childWindow.getTask();
            string _class = childWindow.getClass();
            string folder = "exp\\"+_task + "\\" + _class;
            Directory.CreateDirectory(folder);

            DirectoryInfo dir = new DirectoryInfo(folder);
            int fileNum = dir.GetFiles().Length;
            using (StreamWriter file = File.CreateText(folder + "\\" + fileNum + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, sample);
            }
        }
        public void disconnectEEG()
        {
            connector.Close();
        }

        public void TurnToPrev()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                popupView.ConfirmIsOpen = prev = startCommand = false;
                popupView.TimerText = "";
                countBlink = true;
                //Prev_Button.Focus();
                startCommand = false;
                pdfViewer.GoToPrev();
                typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Next_Button, new object[] { false });
                typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Prev_Button, new object[] { true });
                blinkFre = loop = 0;
                stopwatch.Reset();
            }));
            if (!childWindow.getPopupIsChecked() && childWindow.getAudioIsChecked())
            {
                prevplayer.PlaySync();
            }
        }

        public void TurnToNext()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                popupView.ConfirmIsOpen = next = startCommand = false;
                popupView.TimerText = "";
                countBlink = true;
                startCommand = false;
                //Next_Button.Focus();
                pdfViewer.GoToNext();
                typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Next_Button, new object[] { true });
                typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Prev_Button, new object[] { false });
                blinkFre = loop = 0;
                stopwatch.Reset();
            }));
            if (!childWindow.getPopupIsChecked() && childWindow.getAudioIsChecked())
            {
                nextplayer.PlaySync();
            }
        }

        public void ExitApplication()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                Environment.Exit(0);
            }));

        }

        public void SetCommand()
        {
            //Prev_Button.Focus();
            countBlink = false;
            startCommand = true;
            predCommand = new Queue<int>();
            command = 0;
            popupView.TimerText = "";
            popupView.ConfirmTitle = "Information";
            popupView.ConfirmText = "Start Input Command!";
            popupView.ConfirmIsOpen = true;
            blinkFre = loop = 0;

            this.Dispatcher.Invoke((Action)(() =>
            {
                typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Next_Button, new object[] { false });
                typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Prev_Button, new object[] { false });
            }));
        }

        public void SetTurnToPrev()
        {
            
            popupView.ConfirmTitle = "Confirmation";
            popupView.ConfirmText = "Turn to the PREVIOUS page?";
            popupView.TimerText = "";
            popupView.ConfirmIsOpen = true;
            startCommand = false;
            //Prev_Button.Focus();
            //Prev_Button.Background = Brushes.LightGray;
            if (childWindow.getAudioIsChecked())
                prevConfirmPlayer.PlaySync();
            stopwatch.Restart();
            blinkFre = loop = 0;
            prev = true;
        }

        public void SetTurnToNext()
        {
            popupView.ConfirmTitle = "Confirmation";
            popupView.ConfirmText = "Turn to the NEXT page?";
            popupView.TimerText = "";
            popupView.ConfirmIsOpen = true;
            startCommand = false;
            if (childWindow.getAudioIsChecked())
                nextConfirmPlayer.PlaySync();
            //Next_Button.Focus();
            // Next_Button.Background = Brushes.LightGray;                     
            stopwatch.Restart();
            next = true;
            blinkFre = loop = 0;
        }

        public void SetExitApplication()
        {
            closeplayer.Play();
            popupView.ConfirmIsOpen = true;
            popupView.ConfirmText = "Do you want to close the reader?";
            ++close;
        }
        protected override void OnLocationChanged(EventArgs e)
        {
            Popup.HorizontalOffset += 1;
            Popup.HorizontalOffset -= 1;
            PopupInfo.HorizontalOffset += 1;
            PopupInfo.HorizontalOffset -= 1;

            base.OnLocationChanged(e);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { DefaultExt = ".pdf", Filter = "PDF documents (.pdf)|*.pdf" };
            string filename = null;
            dlg.ShowDialog();
            if (!string.IsNullOrEmpty(dlg.FileName))
            {
                filename = dlg.FileName;
            }
            if (!string.IsNullOrEmpty(filename))
            {
                pdfViewer.LoadFile(filename, page);
                Thread.Sleep(1000);
                pdfToFullScreen();

            }

        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewer.IsDisplay())
            {
                pdfViewer.GoToPrev();
            }
            label = "1";
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewer.IsDisplay())
            {
                pdfViewer.GoToNext();
            }
            label = "2";
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            exp = true;
            cloop = 0;
            text = "";
            this.Dispatcher.Invoke((Action)(() =>
            {
                typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Start_Button, new object[] { true });
                typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Stop_Button, new object[] { false });
                //Left.Background = Brushes.Red;
                // Right.Background = Brushes.Blue;
            }));
            SetCommand();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            exp = false;
            /* string path = @"C:\Users\WANG XI\Exp2\exp_color.txt";
             //int index = 0;
             //string f;
             this.Dispatcher.Invoke((Action)(() =>
             {
                 typeof(System.Windows.Controls.Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Stop_Button, new object[] { true });
             }));
             if (File.Exists(path))
             {
                 using (StreamWriter sw = File.AppendText(path))
                 {
                     sw.Write(text);
                 }
             }
             else
             {
                 using (StreamWriter sw = File.CreateText(path))
                 {
                     sw.WriteLine(title);
                     sw.Write(text);
                 }
             } */

            // System.IO.File.WriteAllText(f, text);
        }

        private void Read_Click(object sender, RoutedEventArgs e)
        {
            label = "0";
        }


        private void showCurrentPage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (showCurrentPage.Text != "")
                pdfViewer.SetPage(Int32.Parse(showCurrentPage.Text));
        }


        private void confirmCommand(int command)
        {
            switch (command)
            {
                case 0:
                    SetTurnToPrev();
                    break;
                case 1:
                    SetTurnToNext();
                    break;
                default:
                    break;
            }

        }
        private void activateCommand(int command)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                switch (command)
                {
                    case 0:
                        Prev_Button.Background = Brushes.Red;
                        Next_Button.Background = Brushes.LightGray;
                        break;
                    case 1:
                        Prev_Button.Background = Brushes.LightGray;
                        Next_Button.Background = Brushes.Red;
                        //Prev_Button.Width = 80;
                        break;
                    default:
                        break;
                }

            }));
        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int p = Int32.Parse(pdfViewer.currentPage);
            int tp = Int32.Parse(pdfViewer.totalPage);
            if (e.Delta > 0 && p > 1)
            {
                pdfViewer.currentPage = (p - 1).ToString();
            }
            else if (e.Delta < 0 && p < tp)
            {
                pdfViewer.currentPage = (p + 1).ToString();
            }
        }

        void MainWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Focus();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public void pdfToFullScreen()
        {
            InputSimulator inputSimulator = new InputSimulator();

            //inputSimulator.Mouse.LeftButtonClick();
            mouse_event(MOUSEEVENTF_LEFTDOWN, 500, 150, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 500, 150, 0, 0);
            inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_L);

        }


    }
}
