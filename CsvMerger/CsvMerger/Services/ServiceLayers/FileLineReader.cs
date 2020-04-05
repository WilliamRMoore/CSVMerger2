﻿using CsvMerger.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static CsvMerger.Services.ServiceLayers.FileLineReader;
using static CsvMerger.Services.ServiceLayers.MapSetService;

namespace CsvMerger.Services.ServiceLayers
{
    public class FileLineReader : IFileLineReader
    {
        private readonly IFileStreamProvider _fileStreamProvider;
        private readonly IRowProcessor _rowProcessor;
        private readonly IPercentageCounter _percentageCounter;

        public FileLineReader(IFileStreamProvider fileStreamProvider, IRowProcessor rowProcessor, IPercentageCounter percentageCounter)
        {
            _fileStreamProvider = fileStreamProvider;
            _rowProcessor = rowProcessor;
            _percentageCounter = percentageCounter;
        }

        public long CountLines(string filePath)
        {
            var reader = _fileStreamProvider.GetReadStream(filePath);
            long count = 0;
            reader.ReadLine();

            while (reader.EndOfStream != true)
            {
                count += 1;
                reader.ReadLine();
            }

            reader.Close();

            return count;
        }

        //TODO inject IPercentCalc
        public List<string> LineReader(string filePath, string[] resultArray, List<int[]> mappingRules)
        {
            List<string> resultFileRows = new List<string>();
            string[] attributes;
            var file = _fileStreamProvider.GetReadStream(filePath);
            var lineCount = CountLines(filePath);
            file.ReadLine();

            for(long i = lineCount; i > 0; i--)
            {
                attributes = _rowProcessor.RowSplitter(file.ReadLine());
                resultArray = _rowProcessor.RowMapper(mappingRules, attributes, resultArray);
                resultFileRows.Add(string.Join(",", resultArray));
                _percentageCounter.CalcPercent();
            }

            file.Close();
            return resultFileRows;
        }
    }
}
