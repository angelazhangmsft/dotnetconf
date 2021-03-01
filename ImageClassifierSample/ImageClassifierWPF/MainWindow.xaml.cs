using ImageClassifier;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.AI.MachineLearning;
using Windows.Media;

namespace ImageClassifierWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string filePath;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            // Upload a picture to the ImageViewer 
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            op.InitialDirectory = @"c:\demo\";
            if (op.ShowDialog() == true)
            {
                filePath = op.FileName;
                Output.Text = "";
                ImageViewer.Source = new BitmapImage(new Uri(filePath));
            }
        }

        private async void DetectButton_Click(object sender, RoutedEventArgs e)
        {
            // Load model
            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var squeezeNetModel = SqueezeNetModel.CreateFromFilePath(Path.Combine(rootDir, "squeezenet1.0-9.onnx"));

            // Load labels from JSON
            var labels = new List<string>();
            foreach (var kvp in JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(rootDir, "Labels.json"))))
            {
                labels.Add(kvp.Value);
            }

            // Open image file
            SqueezeNetOutput output;
            using (var fileStream = File.OpenRead(filePath))
            {
                // Convert from FileStream to ImageFeatureValue
                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(fileStream.AsRandomAccessStream());
                using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                using var inputImage = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
                var imageTensor = ImageFeatureValue.CreateFromVideoFrame(inputImage);

                output = await squeezeNetModel.EvaluateAsync(new SqueezeNetInput
                {
                    data_0 = imageTensor
                });
            }

            // Get result, which is a list of floats with all the probabilities for all 1000 classes of SqueezeNet
            var resultTensor = output.softmaxout_1;
            var resultVector = resultTensor.GetAsVectorView();

            // Order the 1000 results with their indexes to know which class is the highest ranked one
            List<(int index, float p)> results = new List<(int, float)>();
            for (int i = 0; i < resultVector.Count; i++)
            {
                results.Add((index: i, p: resultVector.ElementAt(i)));
            }
            results.Sort((a, b) => a.p switch
            {
                var p when p < b.p => 1,
                var p when p > b.p => -1,
                _ => 0
            });

            // Display classification with probability
            Output.Text = $"Image '{filePath}' is classified as '{labels[results[0].index]}'(p={(int)(results[0].p * 100)}%).";
        }
    }
}
