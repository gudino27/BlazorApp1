using Microsoft.Playwright;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BlazorApp1.Services
{
    public class Degree
    {
        public string? DegreeDescription { get; set; }
        /**********************************************************************************
        *   function name: PrintDegreInfo                                                 *
        *   precondition: DegreeDescription may be null or contain degree summary text    *
        *   Parameters: none                                                              *
        *   description: Prints the degree description to the console                     *
        *   Post-condition: Degree description is printed                                 *
        *   Returns: void                                                                 *
        ***********************************************************************************/
        public virtual void PrintDegreeInfo()
        {
            Console.WriteLine(DegreeDescription);
        }
    }

    public class Major : Degree
    {
        public string Header { get; set; } = "";
        public string Description { get; set; } = "";

        public class CourseRequirement
        {
            public string Name { get; set; } = "";
            public string Credits { get; set; } = "";
        }

        public List<CourseRequirement> CourseRequirements { get; } = new();
        public int? CreditRequirement { get; set; }

        public List<string> StructuredOutput { get; set; } = new();
        public Dictionary<string, string> Footnotes { get; set; } = new();
        public int TotalCredits { get; set; } = 0;

        /***********************************************************************************
         *   function name: PrintDegreInfo                                                 *
         *   precondition: StructuredOutput and Footnotes are populated                    *
         *   Parameters: none                                                              *
         *   description: Prints the degree description, course sequence, and footnotes    *
         *   Post-condition: Degree info is printed                                        *
         *   Returns: void                                                                 *
         ***********************************************************************************/
        public override void PrintDegreeInfo()
        {
            Console.WriteLine(Header);
            if (!string.IsNullOrWhiteSpace(Description))
            {
                Console.WriteLine(Description);
            }
            if (CreditRequirement.HasValue)
            {
                Console.WriteLine($"Total credits required: {CreditRequirement.Value}");
            }
            foreach (var line in StructuredOutput)
                Console.WriteLine(line);

            if (Footnotes.Count > 0)
            {
                Console.WriteLine("\n--- Footnotes ---");
                foreach (var kvp in Footnotes)
                    Console.WriteLine($"[{kvp.Key}]: {kvp.Value}");
            }
        }
    }
    public class DegreeTask
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class Minor : Degree

    {
        public string Header { get; set; } = "";
        public int TotalCredits { get; set; }
        public int UpperDivisionCredits { get; set; }
        public double? MinimumGPA { get; set; }
        public List<string> Courses { get; set; } = new();
        public List<string> Notes { get; set; } = new();
        public List<string> StructuredContent { get; set; } = new();

        public void ParseAdditionalContent(string text)
        {
            // total‐credits
            var m1 = Regex.Match(text, @"(?<c>\d{1,3})\s+credits?", RegexOptions.IgnoreCase);
            if (m1.Success && int.TryParse(m1.Groups["c"].Value, out var tot))
            {
                TotalCredits = tot;
                StructuredContent.Add($"Minimum credits: {tot}");
            }

            // upper‐division
            var m2 = Regex.Match(text, @"(?<u>(Nine|Ten|Eleven|Twelve|\d+))\s+credits.*?(upper-division|300-400-level)", RegexOptions.IgnoreCase);
            if (m2.Success)
            {
                var val = m2.Groups["u"].Value.ToLower();
                var up = val switch
                {
                    "nine" => 9,
                    "ten" => 10,
                    "eleven" => 11,
                    "twelve" => 12,
                    _ => int.TryParse(val, out var x) ? x : 0
                };
                UpperDivisionCredits = up;
                StructuredContent.Add($"upper-division credits: {up}");
            }

            // GPA
            var m3 = Regex.Match(text, @"\b(\d\.\d+)\b");
            if (m3.Success && double.TryParse(m3.Value, out var gpa))
            {
                MinimumGPA = gpa;
                StructuredContent.Add($"minimum Cumalitive GPA: {gpa:F1}");
            }

            // courses
            var courseRx = new Regex(@"([A-Z/&\s]{2,})\s*(\d{3})");
            foreach (Match c in courseRx.Matches(text))
            {
                var course = Regex.Replace(c.Value, @"\s+", " ").Trim();
                if (!Courses.Contains(course))
                    Courses.Add(course);
            }
        }
        public override void PrintDegreeInfo()
        {
            Console.WriteLine("[MINOR DEBUG PREVIEW]\n======================");
            Console.WriteLine($"Minor: {Header}");
            Console.WriteLine($"Description: {DegreeDescription}");

            if (TotalCredits > 0)
                Console.WriteLine($"Minimum credits: {TotalCredits}");
            if (UpperDivisionCredits > 0)
                Console.WriteLine($"upper-division credits: {UpperDivisionCredits}");
            if (MinimumGPA.HasValue)
                Console.WriteLine($"minimum Cumulative  GPA: {MinimumGPA:F1}");

            foreach (var course in Courses.Distinct())
                Console.WriteLine($"Course: {course}");
            foreach (var note in Notes.Distinct())
            {
                if (note.Contains("&nbsp;"))
                {
                    Console.WriteLine("Note:" + note.Replace("&nbsp;", ""));
                }
                else
                {
                    Console.WriteLine("Note:" + note.Trim());
                }
            }

            Console.WriteLine("\n----------------------------\n");
        }
    }
    public class DegreeScrape
    {
        public static List<Degree> degreeList = new();
        
        /// <summary>
        /// Scrapes all degrees from the WSU catalog using Playwright
        /// </summary>

        public static void Main(string[] args)
        {
            // Call the async method and wait for it to complete
            ScrapeAll().GetAwaiter().GetResult();
        }
        /**********************************************************************************
         *   function name: ScrapeAll                                                     *
         *   precondition: WSU catalog site must be online                                *
         *   Parameters: none                                                             *
         *   description: Loads and processes all degrees in parallel using fixed browser *
         *                instances that persist throughout the process                   *
         *   Post-condition: All degree data collected in memory                          *
         *   Returns: void                                                                *
         **********************************************************************************/
        public static async Task ScrapeAll()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Initialize Playwright
                using var playwright = await Playwright.CreateAsync();

                // Launch browser
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[] {
                        "--disable-gpu",
                        "--disable-dev-shm-usage",
                        "--disable-setuid-sandbox",
                        "--no-sandbox"
                    }
                });

                // Get all degrees to scrape
                var allTasks = await GetAllDegreesAsync(browser);
                Console.WriteLine($"Found {allTasks.Count} degrees to scrape");

                // Determine optimal number of parallel tasks
                int coreCount = Environment.ProcessorCount;
                int numParallel = Math.Max(25, Math.Min(8, (coreCount / 2) + 1));
                Console.WriteLine($"Detected {coreCount} logical processors. Using {numParallel} parallel tasks.\n");

                // Create a thread-safe collection for results
                var results = new ConcurrentBag<Degree>();
                var failedTasks = new ConcurrentBag<DegreeTask>();

                // Divide degrees among instances
                var taskBatches = CreateBatches(allTasks, numParallel);

                // Process batches in parallel
                await Parallel.ForEachAsync(taskBatches, new ParallelOptions { MaxDegreeOfParallelism = numParallel },
                    async (batch, token) =>
                    {
                        int instanceId = taskBatches.IndexOf(batch);
                        try
                        {
                            await ProcessBatchAsync(browser, batch, instanceId, results, failedTasks);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[FATAL ERROR] Instance {instanceId} crashed: {ex.Message}");
                            Console.ResetColor();
                        }
                    });

                // Retry failed tasks
                if (failedTasks.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n===== RETRYING {failedTasks.Count} FAILED TASKS =====");
                    Console.ResetColor();

                    var retryBatches = CreateBatches(failedTasks.ToList(), numParallel);

                    await Parallel.ForEachAsync(retryBatches, new ParallelOptions { MaxDegreeOfParallelism = numParallel },
                        async (batch, token) =>
                        {
                            int instanceId = retryBatches.IndexOf(batch) + 100;
                            try
                            {
                                await ProcessBatchAsync(browser, batch, instanceId, results, new ConcurrentBag<DegreeTask>(), true);
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"[RETRY ERROR] Instance {instanceId} crashed: {ex.Message}");
                                Console.ResetColor();
                            }
                        });
                }

                Console.WriteLine("\n===== ALL DONE =====");
                degreeList.AddRange(results);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FATAL ERROR] {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
                Console.ResetColor();
            }
            finally
            {
                stopwatch.Stop();
                string elapsedTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[INFO] Program completed in {elapsedTime}");
                Console.ResetColor();
                Console.WriteLine($"Total degrees scraped: {degreeList.Count}");
                Console.WriteLine($"Total majors completed: {degreeList.Count(d => d is Major)}");
                Console.WriteLine($"Total minors completed: {degreeList.Count(d => d is Minor)}");
            }
        }
        /**********************************************************************************
         *   function name: CreateBatches                                                 *
         *   precondition: tasks is a list of DegreeTask objects                          *
         *   Parameters: tasks - List of DegreeTask objects, batchCount - number of batches*
         *   description: Divides the tasks into smaller batches for parallel processing  *
         *   Post-condition: Batches are created and returned as a list                   *
         *   Returns: List of batches, each containing a list of DegreeTask objects       *
         **********************************************************************************/
        private static List<List<DegreeTask>> CreateBatches(List<DegreeTask> tasks, int batchCount)
        {
            var batches = new List<List<DegreeTask>>();
            int batchSize = (int)Math.Ceiling((double)tasks.Count / batchCount);

            for (int i = 0; i < batchCount; i++)
            {
                int startIndex = i * batchSize;
                int count = Math.Min(batchSize, tasks.Count - startIndex);

                // If we've run out of tasks, break
                if (count <= 0) break;

                var batch = tasks.GetRange(startIndex, count);
                batches.Add(batch);
            }
            return batches;
        }
        /***********************************************************************************
         *   function name: ProcessBatchAsync                                              *
         *   precondition: browser is a valid Playwright browser instance                  *
         *   Parameters: browser - Playwright browser instance, batch - list of tasks,     *
         *               instanceId - ID of the current instance, results - collection of  *
         *               results, failedTasks - collection of failed tasks                 *
         *   description: Processes a batch of tasks in parallel using Playwright          *
         *   Post-condition: Results are added to the results collection                   *
         *   Returns: Task representing the asynchronous operation                         *
         ***********************************************************************************/
        private static async Task ProcessBatchAsync(IBrowser browser, List<DegreeTask> batch, int instanceId, ConcurrentBag<Degree> results, ConcurrentBag<DegreeTask> failedTasks, bool isRetry = false)
        {
            Console.WriteLine($"Instance {instanceId} Starting with {batch.Count} degrees to process");
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                JavaScriptEnabled = true
            });
            var page = await context.NewPageAsync();
            bool initialPageLoaded = false;
            for (int attempt = 1; attempt <= 5 && !initialPageLoaded; attempt++)
            {
                try
                {
                    await page.GotoAsync("https://catalog.wsu.edu/Degrees", new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle,
                        Timeout = 60000
                    });
                    await Task.Delay(3000);

                    await page.ReloadAsync();
                    await Task.Delay(4000);
                    await page.WaitForSelectorAsync("#degree-selector", new PageWaitForSelectorOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 30000
                    });
                    initialPageLoaded = true;
                    Console.WriteLine($"[Instance {instanceId}] Page initialized successfully");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Instance {instanceId}] Attempt {attempt} failed to load initial page: {ex.Message}");
                    Console.ResetColor();
                    if (attempt == 5)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[Instance {instanceId}] Failed to initialize browser after 5 attempts. Aborting this batch.");
                        Console.ResetColor();
                        foreach (var task in batch)
                        {
                            failedTasks.Add(task);
                        }
                        await context.DisposeAsync();
                        return;
                    }
                    await Task.Delay(5000);
                }
            }
            int completedCount = 0;
            foreach (var task in batch)
            {
                try
                {
                    await SelectDegreeOptionAsync(page, task.Value);
                    var localDegrees = new List<Degree>();
                    await ProcessScheduleOfStudiesAsync(page, localDegrees);
                    foreach (var degree in localDegrees)
                    {
                        results.Add(degree);
                    }
                    completedCount++;
                    double progress = (double)completedCount / batch.Count * 100;
                    Console.WriteLine($"Thread: {instanceId} has Completed {progress:F1}%");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Instance {instanceId}] Failed: {task.Name} — {ex.Message}");
                    Console.ResetColor();
                    if (!isRetry)
                    {
                        failedTasks.Add(task);
                    }
                    try
                    {
                        await page.GotoAsync("https://catalog.wsu.edu/Degrees", new PageGotoOptions
                        {
                            WaitUntil = WaitUntilState.NetworkIdle,
                            Timeout = 30000
                        });
                        await Task.Delay(3000);
                        await page.ReloadAsync();
                        await Task.Delay(2000);
                    }
                    catch
                    {
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[Instance {instanceId}] Batch completed: {completedCount}/{batch.Count} successful");
            Console.ResetColor();
            await context.DisposeAsync();
        }
        /**********************************************************************************
         *   function name: SelectDegreeOptionAsync                                       *
         *   precondition: page is a valid Playwright page instance                       *
         *   Parameters: page - Playwright page instance, value - degree option value     *
         *   description: Selects a degree option from the dropdown and waits for loading *
         *   Post-condition: Degree option is selected and page is loaded                 *
         *   Returns: Task representing the asynchronous operation                        *
         **********************************************************************************/
        private static async Task SelectDegreeOptionAsync(IPage page, string value)
        {
            await page.WaitForSelectorAsync("#degree-selector", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });
            await page.SelectOptionAsync("#degree-selector", value);
            await Task.Delay(5000);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        /**********************************************************************************
         *   function name: GetAllDegreesAsync                                            *
         *   precondition: browser is a valid Playwright browser instance                 *
         *   Parameters: browser - Playwright browser instance                            *
         *   description: Retrieves all degree options from the WSU catalog               *
         *   Post-condition: Degree options are retrieved and returned                    *
         *   Returns: List of DegreeTask objects representing degree options              *
         **********************************************************************************/
        private static async Task<List<DegreeTask>> GetAllDegreesAsync(IBrowser browser)
        {
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await page.GotoAsync("https://catalog.wsu.edu/Degrees", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 60000
            });
            for (int i = 0; i < 3; i++)
            {
                await page.ReloadAsync();
                await Task.Delay(2000);
            }
            await page.WaitForSelectorAsync("#degree-selector", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });
            var options = await page.QuerySelectorAllAsync("#degree-selector option");
            var tasks = new List<DegreeTask>();
            for (int i = 1; i < options.Count; i++)
            {
                var opt = options[i];
                var text = await opt.TextContentAsync();
                var value = await opt.GetAttributeAsync("value");
                tasks.Add(new DegreeTask
                {
                    Name = text.Trim(),
                    Value = value
                });
            }
            await context.DisposeAsync();
            return tasks;
        }
        /**********************************************************************************
         *   function name: ProcessScheduleOfStudiesAsync                                 *
         *   precondition: page is a valid Playwright page instance                       *
         *   Parameters: page - Playwright page instance, localDegrees - list of degrees  *
         *   description: Processes the schedule of studies for each degree to get the    *
         *   major                                                                        *
         *   Post-condition: majors are processed and added to the localDegrees list      *
         *   Returns: Task representing the asynchronous operation                        *
         **********************************************************************************/
        private static async Task ProcessScheduleOfStudiesAsync(IPage page, List<Degree> localDegrees)
        {
            try
            {
                await page.WaitForSelectorAsync("nav.secondary_nav", new PageWaitForSelectorOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });
                var scheduleHeadings = await page.QuerySelectorAllAsync("nav.secondary_nav h5");
                string[] targets = { "Schedule of Studies", "Minors" };
                foreach (var heading in scheduleHeadings)
                {
                    string headingText = await heading.TextContentAsync();
                    if (!targets.Contains(headingText, StringComparer.OrdinalIgnoreCase)) continue;
                    try
                    {
                        var ulElement = await heading.EvaluateHandleAsync("el => el.nextElementSibling");
                        var degreeLinks = await ulElement.AsElement().QuerySelectorAllAsync("li.secnav_li");
                        foreach (var link in degreeLinks)
                        {
                            string label = await link.TextContentAsync();

                            await link.ClickAsync();

                            await Task.Delay(4000); // Allow time for content to load
                            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                            if (headingText.Equals("Minors", StringComparison.OrdinalIgnoreCase))

                            {
                                Console.WriteLine($"Processing minor: {label}");
                                // Fix for CS0119, CS1003, and CS0103 errors
                                // The issue is with the incorrect use of the `Any` method and the lambda expression.
                                // Correcting the lambda parameter and ensuring proper syntax.

                                if (localDegrees.Any(d => d is Minor minor && minor.Header.Equals(label, StringComparison.OrdinalIgnoreCase)))
                                {
                                    Console.WriteLine($"Minor {label} already exists in the list. Skipping.");
                                    continue;
                                }
                                await ProcessMinorDegreePageAsync(page, localDegrees, label);
                                
                            }
                            else
                            {
                                Console.WriteLine($"Processing major: {label}");
                                // Check if the major is already in the list
                                if (localDegrees.Any(d => d is Major major&& major.Header.Equals(label, StringComparison.OrdinalIgnoreCase)))
                                {
                                    Console.WriteLine($"Major {label} already exists in the list. Skipping.");
                                    continue;
                                }
                                await ProcessDynamicDegreePageAsync(page, localDegrees);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing {headingText}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) when (ex.Message.Contains("timeout"))
            {
                Console.WriteLine($"Timeout waiting for secondary_nav: {ex.Message}");
                var content = await page.ContentAsync();
                if (content.Contains("unit-fullinfo"))
                {
                    try
                    {
                        // Try to process what we can directly
                        await ProcessDynamicDegreePageAsync(page, localDegrees);
                    }
                    catch { Console.WriteLine("process dynamic page failed"); }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessScheduleOfStudiesAsync: {ex.Message}");
            }
        }
        /**********************************************************************************
         *   function name: ProcessMinorDegreePageAsync                                   *
         *   precondition: page is a valid Playwright page instance                       *
         *   Parameters: page - Playwright page instance, degrees - list of degrees,      *
         *               minorName - name of the minor                                    *
         *   description: Processes the minor degree page and extracts information        *
         *   Post-condition: Minor degree information is extracted and added to the list  *
         *   Returns: Task representing the asynchronous operation                        *
         **********************************************************************************/
        private static async Task ProcessMinorDegreePageAsync(IPage page, List<Degree> degrees, string minorName)
        {
            await page.WaitForSelectorAsync(".unit-fullinfo", new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            var contentHtml = await page.InnerHTMLAsync(".unit-fullinfo");
            Minor minor = ParseMinorHtml(contentHtml, minorName);
            degrees.Add(minor);
        }
        /**********************************************************************************
         *   function name: ParseMinorHtml                                                *
         *   precondition: rawHtml is a valid HTML string                                 *
         *   Parameters: rawHtml - HTML string, minorName - name of the minor             *
         *   description: Parses the HTML content and extracts minor information          *
         *   Post-condition: Minor information is extracted and returned                  *
         *   Returns: Minor object containing parsed information                          *
         **********************************************************************************/
        public static Minor ParseMinorHtml(string rawHtml, string minorName)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(rawHtml);

            var minor = new Minor { Header= minorName };

            // Grab all inner divs without a class (content paragraphs)
            var blocks = doc.DocumentNode
                .SelectNodes("//div[not(@class)]")
                ?.Select(d => d.InnerText.Trim())
                .Where(txt => !string.IsNullOrWhiteSpace(txt))
                .ToList() ?? new List<string>();

            foreach (var block in blocks)
            {
                minor.Notes.Add(block);
                minor.ParseAdditionalContent(block);
            }

            return minor;
        }
        /**********************************************************************************
         *   function name: ProcessDynamicDegreePageAsync                                 *
         *   precondition: page is a valid Playwright page instance                       *
         *   Parameters: page - Playwright page instance, degrees - list of degrees       *
         *   description: Processes the dynamic degree page and extracts information      *
         *   Post-condition: Degree information is extracted and added to the list        *
         *   Returns: Task representing the asynchronous operation                        *
         **********************************************************************************/
        private static async Task ProcessDynamicDegreePageAsync(IPage page, List<Degree> degrees)
        {
            try
            {
                // Get the main content element
                var content = await page.QuerySelectorAsync(".unit-fullinfo");

                // 1) Grab the header element
                var headerEl = await content.QuerySelectorAsync("h4.academics_header > span");
                var rawHeader = await headerEl.TextContentAsync();

                // E.g. "Accounting (120 Credits)"
                var m = Regex.Match(rawHeader,
                    @"^(?<name>.*?)\s*\(\s*(?<cr>\d+)\s+Credits?\s*\)$",
                    RegexOptions.IgnoreCase);

                var major = new Major();
                if (m.Success)
                {
                    major.Header = rawHeader;
                    major.CreditRequirement = int.Parse(m.Groups["cr"].Value);
                    // Store the bare name
                    major.DegreeDescription = m.Groups["name"].Value.Trim();
                }
                else
                {
                    major.Header = rawHeader;
                    major.DegreeDescription = rawHeader;
                }

                // 3) Grab the description
                try
                {
                    var descNode = await page.QuerySelectorAsync("h4.academics_header + div");
                    if (descNode != null)
                    {
                        major.Description = await descNode.TextContentAsync();
                    }
                }
                catch
                {
                    major.Description = "";
                }

                // 4) Process the sequence table
                var tableRows = await page.QuerySelectorAllAsync("table.degree_sequence_list > tr");
                foreach (var row in tableRows)
                {
                    var tds = await row.QuerySelectorAllAsync("td");
                    if (tds.Count == 0) continue;

                    var tdClass = await tds[0].GetAttributeAsync("class") ?? "";

                    if (tdClass.Equals("degree_sequence_Year", StringComparison.OrdinalIgnoreCase))
                    {
                        major.StructuredOutput.Add($"{await tds[0].TextContentAsync()}");
                        continue;
                    }

                    if (tdClass.Contains("degree_sequence_term"))
                    {
                        major.StructuredOutput.Add($"{await tds[0].TextContentAsync()}");
                        continue;
                    }

                    if (tds.Count >= 2)
                    {
                        var courseText = await tds[0].TextContentAsync();
                        var creditText = await tds[1].TextContentAsync();

                        if (!string.IsNullOrWhiteSpace(courseText))
                        {
                            major.StructuredOutput.Add($"Course: {courseText}, Credits: {creditText}");
                            major.CourseRequirements.Add(new Major.CourseRequirement
                            {
                                Name = courseText,
                                Credits = creditText
                            });

                            if (int.TryParse(
                                  Regex.Match(creditText, @"\d+").Value,
                                  out var c))
                                major.TotalCredits += c;
                        }
                    }
                    else if (tds.Count == 1)
                    {
                        var messageText = await tds[0].TextContentAsync();
                        if (!string.IsNullOrWhiteSpace(messageText))
                            major.StructuredOutput.Add($"Message: {messageText}");
                    }
                }

                // 5) Process footnotes
                var footnoteRows = await page.QuerySelectorAllAsync("table.degree_sequence_footnotes > tr");
                foreach (var row in footnoteRows)
                {
                    var cells = await row.QuerySelectorAllAsync("td");
                    if (cells.Count < 2) continue;

                    var key = Regex.Replace(await cells[0].TextContentAsync(), "[^\\d]", "");
                    var value = await cells[1].TextContentAsync();

                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        major.Footnotes[key] = value;
                }

                degrees.Add(major);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Error processing dynamic degree page: {ex.Message}");
                Console.ResetColor();

                // Try to extract what we can from the page source
                try
                {
                    var htmlSource = await page.ContentAsync();
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlSource);

                    // Try to get the header at minimum
                    var headerNode = htmlDoc.DocumentNode.SelectSingleNode("//h4[@class='academics_header']/span");
                    if (headerNode != null)
                    {
                        var major = new Major();
                        major.Header = headerNode.InnerText.Trim();
                        major.DegreeDescription = major.Header;

                        // Try to extract credit requirement from header
                        var m = Regex.Match(major.Header, @"\((?<cr>\d+)\s+Credits?\)", RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            major.CreditRequirement = int.Parse(m.Groups["cr"].Value);
                        }

                        major.StructuredOutput.Add("ERROR: Page could not be fully processed. Partial data only.");
                        degrees.Add(major);
                    }
                }
                catch
                {
                    // If all else fails, at least add something to indicate we tried this degree
                    var fallbackMajor = new Major();
                    fallbackMajor.DegreeDescription = "Error processing page";
                    fallbackMajor.Header = "Error processing page";
                    fallbackMajor.StructuredOutput.Add("ERROR: Could not process this degree page.");
                    degrees.Add(fallbackMajor);
                }
            }
        }
    }
}


