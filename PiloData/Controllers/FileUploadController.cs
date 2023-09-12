using Microsoft.AspNetCore.Mvc;
using PiloData.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Microsoft.AspNetCore.Hosting.Server;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Collections;
using System;

namespace PiloData.Controllers
{
    public class FileUploadController : Controller
    {

        static List<FromTextFile> fromTextFiles;
        static FileInfo fileInfo;
        static int countNoOfRows;
        static List<GroupByModel> listone;
        static List<GroupByModel> listone1;
        static List<RangedSortedList> rangedSortedLists;
        static bool Success = true;
        static int totalNoOfRows;
        static int FindStartRange;
        static int FindEndRange;
        static SingleFileModel model;
        static List<ThousandIncModel> thousandIncModels;

        public IActionResult Index()
        {
            SingleFileModel model = new SingleFileModel();           
            return View(model);
        }

        [HttpPost]
        public IActionResult Upload(SingleFileModel model)
        {
            if (model.File != null)
            {
                if(model.StartRange == null || model.EndRange == null)
                {
                    model.IsSuccess = false;
                    ViewBag.Success = "false";
                    ViewBag.Message = "Please select range.";                    
                    return View("Index");
                }
                model.IsResponse = true;
                model.IsSuccess = true;


                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files");

                //create folder if not exist
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                fileInfo = new FileInfo(model.File.FileName);

                if(fileInfo.Extension == ".txt")
                {
                    //filePath = model.File.FileName;
                    string filePath = fileInfo.DirectoryName + "\\" + fileInfo.Name;

                    string fileNameWithPath = Path.Combine(path, fileInfo.Name);

                    //getting network number
                    string[] NetworkNumber = fileInfo.Name.Split(" ");
                    model.NetworkNumber = NetworkNumber[0];

                    //getting inserted date and time              
                    DateTime currentDateTime = DateTime.Now;
                    model.InsertedTime = currentDateTime;

                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        model.File.CopyTo(stream);
                    }

                    IEnumerable<string> texts = System.IO.File.ReadLines(fileNameWithPath);
                    string[] thisone = texts.ToArray();

                    //initializations
                    rangedSortedLists = SwappedAndSorted(thisone, model);        

                    if(Success == false)
                    {                       
                        ViewBag.Success = "false";
                        ViewBag.Message = "Start/End range does not exist.";
                        if (FindStartRange > FindEndRange)
                        {
                            ViewBag.Message = "End range is higher than start range.";
                        }
                        Success = true;
                        return View("Index");
                    }

                    listone = rangedSortedLists.GroupBy(x => new { x.Prefix_ENTNAHME, x.Suffix_ENTNAHME }, (key, group) => new GroupByModel()
                    {
                        SuffixGroup = key.Suffix_ENTNAHME,
                        PrefixGroup = key.Prefix_ENTNAHME,
                        //MidfixGroup = key.Middle_ENTNAHME,
                        Key = group.Count()
                    }).ToList();

                    listone1 = rangedSortedLists.GroupBy(x => new { x.Prefix_ENTNAHME, x.Suffix_ENTNAHME, x.Middle_ENTNAHME }, (key, group) => new GroupByModel()
                    {
                        SuffixGroup = key.Suffix_ENTNAHME,
                        PrefixGroup = key.Prefix_ENTNAHME,
                        MidfixGroup = key.Middle_ENTNAHME,
                        Key = group.Count()
                    }).ToList();

                    ViewBag.DataGrouped = listone;
                    ViewBag.Data = fromTextFiles;
                    model.TotalNoOfRows = totalNoOfRows;
                    model.NoOfRows = countNoOfRows;
                    model.NoOfGroups = listone.Count();
                    model.IsSuccess = true;
                    model.Message = "File upload successfully";
                    model.FilePath = filePath;
                }

                else
                {
                    model.IsSuccess = false;                  
                    ViewBag.Success = "false";
                    ViewBag.Message = "Format of file uploaded should be a .txt";
                    return View("Index");
                }

                return View("UploadSuccessful", model);
            }
            else
            {
                model.IsSuccess = false;                
                ViewBag.Success = "false";
                ViewBag.Message = "Please select a file.";
                return View("Index");
            }            
        }

        [HttpPost]
        public IActionResult Export()
        {         
            List<RangedSortedList> fromTextFile = rangedSortedLists;            
            var sb = new StringBuilder();
            sb.AppendLine("ENTNAHME,ERSATZ,TYP,ERF_USER,ERF_DATUM");

            string fmt = "00000000";
            foreach (var data in fromTextFile)
            {
                sb.AppendLine(data.ENTNAHME + "," + data.ERSATZ + "," + data.TYP + "," + data.ERF_USER + "," + data.ERF_DATUM);
            }

            ViewBag.FromTextFile = fromTextFile;

            string[] getFileName = fileInfo.Name.Split(".txt");
            string fileName = getFileName[0];

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName+".csv");
        }

        public IActionResult Export1000()
        {
            List<ThousandIncModel> fromTextFile = thousandIncModels;
            var sb = new StringBuilder();
            sb.AppendLine("ENTNAHME,ERSATZ,TYP,ERF_USER,ERF_DATUM");

            string fmt = "00000000";
            foreach (var data in fromTextFile)
            {
                sb.AppendLine(data.ENTNAHME + "," + data.ERSATZ + "," + data.TYP + "," + data.ERF_USER + "," + data.ERF_DATUM);
            }

            ViewBag.FromTextFile = fromTextFile;

            string[] getFileName = fileInfo.Name.Split(".txt");
            string fileName = getFileName[0];

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName + " _ 1000" + ".csv");
        }

        public List<RangedSortedList> SwappedAndSorted(string[] thisone, SingleFileModel model) 
        {
            List<FromTextFile> ColumnsSwapped = new List<FromTextFile>();
            countNoOfRows = 0;
            totalNoOfRows = 0;

            for (int i = 1; i < thisone.Length; i++)
            {                
                var words = thisone[i].Split(";"); 
                ColumnsSwapped.Add(new FromTextFile() { ENTNAHME = words[1], ERSATZ = words[0], TYP = words[2], ERF_USER = words[3], ERF_DATUM = words[4] });
                totalNoOfRows++;
            }

            List<FromTextFile> SortedList = ColumnsSwapped
                                            .OrderBy(o => o.Prefix_ENTNAHME)
                                            .ThenBy(o => o.Suffix_ENTNAHME)
                                            .ThenBy(o => o.Middle_ENTNAHME)
                                            .ToList();        

            FromTextFile[] myArray = SortedList.ToArray();

            var query = SortedList
              .Select((x, i) => new { Index = i, x.ENTNAHME, x.ERSATZ, x.TYP, x.ERF_USER, x.ERF_DATUM })
              .ToList();

            FindStartRange = query.Where(o => o.ENTNAHME.Contains(model.StartRange)).Select(a => a.Index).FirstOrDefault();
            FindEndRange = query.Where(o => o.ENTNAHME.Contains(model.EndRange)).Select(a => a.Index).FirstOrDefault();

            string GetStartRange = "";
            string GetLastRange = "";

            GetStartRange = SortedList.Where(o => o.ENTNAHME.Contains(model.StartRange)).Select(a => a.ENTNAHME).FirstOrDefault();
            GetLastRange = SortedList.Where(o => o.ENTNAHME.Contains(model.EndRange)).Select(a => a.ENTNAHME).FirstOrDefault();


            List<RangedSortedList> rangedSortedLists = new List<RangedSortedList>();

            if (GetStartRange == null  || GetLastRange == null || FindStartRange > FindEndRange)
            {
                Success = false;
            }          
            else
            {
                for (int i = FindStartRange; i<= FindEndRange; i++)
                {
                    rangedSortedLists.Add(new RangedSortedList()
                    {
                        INDEX = i,
                        ENTNAHME = query[i].ENTNAHME,
                        ERSATZ = query[i].ERSATZ,
                        TYP = query[i].TYP,
                        ERF_USER = query[i].ERF_USER,
                        ERF_DATUM = query[i].ERF_DATUM
                    });
                    countNoOfRows++;     
                }
            }
            return rangedSortedLists;          
        }      

        //[HttpPost]
        public int CheckThousandNum(int firstnum, int lastnum, List<GroupByModel> internalgroup)
        {
            //Console.WriteLine(firstnum.ToString() + "-" + lastnum.ToString());
            //int count = 0;

            int getFirstIndex = rangedSortedLists.Where(x => x.Middle_ENTNAHME == firstnum).Select(x => x.INDEX).First();
            int getLastIndex = getFirstIndex + 998;


            var checkLastIndex = rangedSortedLists.Where(x => x.INDEX == getLastIndex).Select(x => x.Middle_ENTNAHME).FirstOrDefault();
         
            if (checkLastIndex == lastnum)
            {
                for (int n = getFirstIndex; n <= getLastIndex; n++)
                {
                    var getLastOfThousandVars = rangedSortedLists.Where(x => x.INDEX == n).Select(x => new { x.ENTNAHME, x.ERSATZ, x.TYP, x.ERF_USER, x.ERF_DATUM }).First();
                    thousandIncModels.Add(new ThousandIncModel { ENTNAHME = getLastOfThousandVars.ENTNAHME, ERSATZ = getLastOfThousandVars.ERSATZ, TYP = getLastOfThousandVars.TYP, ERF_USER = getLastOfThousandVars.ERF_USER, ERF_DATUM = getLastOfThousandVars.ERF_DATUM });
                   
                }
                return 1;
            }
            else
            {
                return 0;
            }
        }

        [HttpPost]
        public IActionResult CheckIncremental()
        {
            List<List<GroupByModel>> testnew = listone1
                                               .Where(x => x.MidfixGroupString.Contains("001"))
                                               .OrderBy(x => x.PrefixGroup)
                                               .ThenBy(x => x.SuffixGroup)
                                               .GroupBy(x => x.MidfixGroup)
                                               .Select(grp => grp.ToList()).ToList();
          
            int ifNotTrue = 0;
            int indexOfThousands = 1;
            
            thousandIncModels = new List<ThousandIncModel>();

            if (testnew.Count > 0)
            {
                for (int i = 0; i < testnew.Count; i++)
                {                   
                    for (int j = 0; j < testnew[i].Count; j++)
                    {
                        Console.WriteLine(j);
                        int result = CheckThousandNum(testnew[i][j].MidfixGroup, testnew[i][j].MidfixGroup + 998, testnew[i]);
                        if (result != 0)
                        {
                            ifNotTrue++;
                        }     
                    }
                }

                if(ifNotTrue  == 0)
                {
                    ViewBag.Check = "No ascending by 1 up to 1000 times";
                    ViewBag.Export = false;
                }
                else
                {
                    ViewBag.ListOf1000 = thousandIncModels;
                    ViewBag.Check = "Ascending by 1 up to 1000 times detected";
                    ViewBag.Export = true;
                    //return View("CheckIncremental");
                }
            }
            else
            {
                ViewBag.Export = false;
                ViewBag.Check = "No ascending by 1 up to 1000 times";
            }

            return View("CheckIncremental");
        }
    }
}
