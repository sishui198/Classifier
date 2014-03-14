﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ObjectDetect
{
    public class FileAccess
    {
        public struct Pair
        {
            public readonly Uri File;
            public readonly List<rectangle> Rectangles;

            public Pair(Uri file, List<rectangle> rectangles)
            {
                File = file;
                Rectangles = rectangles;
            }
        }

        //public const int smallestWindow = 64, biggestWindow = 512, windowStep = 4, offsetStep = 6, imageWidth = 5184, imageHeight = 3456;
        public static async Task<List<Pair>> loadInfo(string dataFileName)
        {
            List<Pair> fileList = new List<Pair>();

            try
            {
                using (var dataFile = new StreamReader(dataFileName))
                {
                    Uri directory = new Uri(Path.GetDirectoryName(dataFileName) + Path.DirectorySeparatorChar);
                    Uri file = null;
                    //SlidingWindow imageWindow = new SlidingWindow(imageWidth, imageHeight, smallestWindow, biggestWindow, windowStep, offsetStep);

                    int lineNo = -1;
                    for (string line = await dataFile.ReadLineAsync(); line != null; line = await dataFile.ReadLineAsync())
                    {
                        lineNo++;
                        var words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                        if (words.Length < 2) continue;
                        else
                        {
                            file = new Uri(directory, words[0]);
                            if (!System.IO.File.Exists(file.AbsolutePath))
                            {
                                var result = System.Windows.MessageBox.Show("\"" + words[0] + "\" not found in \"" + directory.AbsolutePath + "\"", file.AbsolutePath + " Not Found", System.Windows.MessageBoxButton.OKCancel);
                                if (result == System.Windows.MessageBoxResult.Cancel) return fileList;
                            }
                        }

                        int numSamples;
                        if (!int.TryParse(words[1], out numSamples))
                        {
                            throw new Exception("syntax error on line " + lineNo);
                        }

                        var samples = new List<rectangle>(numSamples);

                        for (int i = 0; i < numSamples; i++)
                        {
                            const int xbase = 2, ybase = 3, wbase = 4, hbase = 5;
                            int x, y, w, h;
                            if (!(int.TryParse(words[xbase + i * 4], out x)
                                & int.TryParse(words[ybase + i * 4], out y)
                                & int.TryParse(words[wbase + i * 4], out w)
                                & int.TryParse(words[hbase + i * 4], out h)))
                            {
                                throw new Exception("syntax error on line " + lineNo + ": error reading sample number " + (i + 1));
                            }
                            samples.Add(new rectangle(x, y, w, h));
                            //double xd, yd, wd, hd;
                            //if (imageWindow.getWindowDimensions(imageWindow.getNearestWindow(x, y, w, h), out xd, out yd, out wd, out hd))
                            //{
                            //    samples[i] = new rectangle(xd, yd, wd, hd);
                            //}
                        }

                        fileList.Add(new Pair(file, samples));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(dataFileName + ": " + e.Message);
            }

            return fileList;
        }

        const int rectStringWidth = 20;
        public static async Task saveInfo(string dataFileName, List<Pair> fileList)
        {
            using (var dataFile = new StreamWriter(dataFileName))
            {
                int maxFilenameLength = 0;
                foreach (var line in fileList)
                {
                    maxFilenameLength = Math.Max(maxFilenameLength, Path.GetFileName(line.File.AbsoluteUri).Length);
                }

                int padding = (maxFilenameLength / rectStringWidth + 1) * rectStringWidth;

                foreach (var line in fileList)
                {
                    await dataFile.WriteAsync(Path.GetFileName(line.File.AbsoluteUri).PadRight(padding) + line.Rectangles.Count.ToString().PadRight(rectStringWidth));

                    foreach (var rect in line.Rectangles)
                    {
                        await dataFile.WriteAsync((rect.Left + " " + rect.Top + " " + rect.Width + " " + rect.Height).PadRight(rectStringWidth));
                    }

                    await dataFile.WriteLineAsync();
                }
            }
        }
    }
}
