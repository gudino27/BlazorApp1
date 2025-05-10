namespace BlazorApp1.Services
{
    using Microsoft.Playwright;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using HtmlAgilityPack;
    using Microsoft.AspNetCore.Routing;
    using PuppeteerSharp.Input;
    using DocumentFormat.OpenXml.Drawing.Diagrams;

    //run this: powershell -ExecutionPolicy Bypass -File bin/Debug/net8.0/playwright.ps1 install
    public class Campus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Term> Terms { get; set; } = new List<Term>();
    }

    public class Term
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<CourseData> Courses { get; set; } = new List<CourseData>();
    }

    public class CourseData
    {
        public string? CourseName { get; set; }
        public string? Title { get; set; }
        public List<SectionData> Sections { get; set; } = new List<SectionData>();
    }

    public class SectionData
    {
        public string? SectionNumber { get; set; }
        public string? Credits { get; set; }
        public string? ClassNumber { get; set; }
        public int SpotsLeft { get; set; }
        public string? Status { get; set; }
        public string? Days { get; set; }
        public string? Time { get; set; }
        public string? Location { get; set; }
        public string? Instructor { get; set; }
        public List<string> CourseDetails { get; set; } = new List<string>();
        public List<CourseDescriptionDetails> CourseDescriptionDetails { get; set; } = new List<CourseDescriptionDetails>();
    }
    public class CourseDescriptionDetails
    {

        public string CourseDescription { get; set; } = string.Empty;
        public string CoursePrerequisite { get; set; } = string.Empty;
        public string CourseCredit { get; set; } = string.Empty;
        public string SpecialCourseFee { get; set; } = string.Empty;
        public string ConsentRequired { get; set; } = string.Empty;
        public string CrosslistedCourses { get; set; } = string.Empty;
        public string ConjointCourses { get; set; } = string.Empty;
        public string UCORE { get; set; } = string.Empty;
        public string GraduateCapstone { get; set; } = string.Empty;
        public string GERCode { get; set; } = string.Empty;
        public string WritingInTheMajor { get; set; } = string.Empty;
        public string Cooperative { get; set; } = string.Empty;
        public List<MeetingsInfo> Meetings { get; set; } = new List<MeetingsInfo>();
        public List<string> Instructors { get; set; } = new List<string>();
        public string InstructionMode { get; set; } = string.Empty;
        public string EnrollmentLimit { get; set; } = string.Empty;
        public string CurrentEnrollment { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Footnotes { get; set; } = string.Empty;

    }
    public class MeetingsInfo
    {
        public string Days { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public override string ToString()
        {
            return $"{Days} {Time} {Location}";
        }
    }
    public class CampusData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TermData
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class StaticDataService
    {
        public List<CampusData> Campuses { get; } = new List<CampusData>
    {
        new CampusData { Id = 1, Name = "Everett" },
        new CampusData { Id = 2, Name = "Global" },
        new CampusData { Id = 3, Name = "Pullman" },
        new CampusData { Id = 4, Name = "Spokane" },
        new CampusData { Id = 5, Name = "Tri-Cities" },
        new CampusData { Id = 6, Name = "Vancouver" }
    };

        public List<TermData> Terms { get; } = new List<TermData>
    {
        new TermData { Code = "fall", Description = "Fall 2025" },
        new TermData { Code = "spring", Description = "Spring 2025" },
        new TermData { Code = "summer", Description = "Summer 2025" }
    };
    }

    partial class CourseScrape
    {

        public static List<Campus> CampusesList { get; set; } = new List<Campus>();
        private static IPlaywright? _playwright;
        private static IBrowser? _browser;

        

        public static async Task Runall()
        {
            var Watch = Stopwatch.StartNew();

            try
            {
                // Initialize Playwright
                _playwright = await Playwright.CreateAsync();

                // Launch browser - uses Chromium by default
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[]
                    {
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--disable-setuid-sandbox",
                "--no-sandbox"
                    }
                });

                var campuses = new Dictionary<int, string>
                {
                    { 1, "Everett" },
                    { 2, "Global" },
                    { 3, "Pullman" },
                    { 4, "Spokane" },
                    { 5, "Tri-Cities" },
                    { 6, "Vancouver" }
                };

                var terms = new Dictionary<int, string>
        {
            { 1, "Fall 2025" },
           { 2, "Spring 2025" },
           { 3, "Summer 2025" }
        };


                // Process each campus and term
                foreach (var (campusId, campusName) in campuses)
                {
                    foreach (var (termId, termName) in terms)
                    {
                        var watch = Stopwatch.StartNew();
                        Console.WriteLine($"Processing {campusName} {termName}");
                        await ProcessCampusAndTerm(campusName, termName);
                        watch.Stop();
                        var elapsedMinutes = watch.ElapsedMilliseconds / 60000;
                        var remainderElapsedSeconds = (watch.ElapsedMilliseconds % 60000) / 1000;
                        Console.WriteLine($"Time elapsed for the term: {elapsedMinutes} minutes and {remainderElapsedSeconds} seconds");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RunAll: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Cleanup
                if (_browser != null)
                {
                    await _browser.DisposeAsync();
                }

                _playwright?.Dispose();
                Watch.Stop();
                var elapsedMinutes = Watch.ElapsedMilliseconds / 60000;
                var remainderElapsedSeconds = (Watch.ElapsedMilliseconds % 60000) / 1000;
                Console.WriteLine($"Total time elapsed: {elapsedMinutes} minutes and {remainderElapsedSeconds} seconds");


            }
        }

        public static async Task DateLoad()
        {
            var watch = Stopwatch.StartNew();
            try
            {
                // Initialize Playwright
                _playwright = await Playwright.CreateAsync();

                // Launch browser - uses Chromium by default
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[]
                    {
                "--disable-gpu",
                "--disable-dev-shm-usage",
                "--disable-setuid-sandbox",
                "--no-sandbox"
                    }
                });

                var campuses = new Dictionary<int, string>
                {
                    { 1, "Everett" },
                    { 2, "Global" },
                    { 3, "Pullman" },
                    { 4, "Spokane" },
                    { 5, "Tri-Cities" },
                    { 6, "Vancouver" }
                };

                var terms = new Dictionary<int, string>
        {
            { 1, "Fall 2025" },
            { 2, "Spring 2025" },
            { 3, "Summer 2025" }
        };
                //get the current date
                DateTime currentDate = DateTime.Now;
                //get the current month
                int currentMonth = currentDate.Month;
                //get the current day
                int currentDay = currentDate.Day;
                //if the date is between august 25th and januaray 12th refresh the spring courses
                // if the date is between january 13th and march 20th refresh the summer courses
                // if the date is between march 21st and august 24th refresh the fall courses
                int august = 8;
                int january = 1;
                int march = 3;
                // Check which term to refresh based on current date
                if ((currentMonth == august && currentDay >= 25) ||
                    (currentMonth > august) ||
                    (currentMonth == january && currentDay <= 12))
                {
                    // Between August 25 and January 12 - refresh Spring courses
                    Console.WriteLine("Refreshing Spring courses");
                    foreach (var (campusId, campusName) in campuses)
                    {
                        await ProcessCampusAndTerm(campusName, "Spring 2025");
                    }
                }
                else if ((currentMonth == january && currentDay > 12) ||
                         (currentMonth > january && currentMonth < march) ||
                         (currentMonth == march && currentDay <= 20))
                {
                    // Between January 13 and March 20 - refresh Summer courses
                    Console.WriteLine("Refreshing Summer courses");
                    foreach (var (campusId, campusName) in campuses)
                    {
                        await ProcessCampusAndTerm(campusName, "Summer 2025");
                    }
                }
                else
                {
                    // Between March 21 and August 24 - refresh Fall courses
                    Console.WriteLine("Refreshing Fall courses");
                    foreach (var (campusId, campusName) in campuses)
                    {
                        await ProcessCampusAndTerm(campusName, "Fall 2025");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error in CourseRefresh: {ex.Message}");

            }
            finally
            {
                if (_browser != null)
                {
                    await _browser.DisposeAsync();
                }

                _playwright?.Dispose();

                watch.Stop();
                var elapsedMinutes = watch.ElapsedMilliseconds / 60000;
                var remainderElapsedSeconds = (watch.ElapsedMilliseconds % 60000) / 1000;
                Console.WriteLine($"Time elapsed: {elapsedMinutes} minutes and {remainderElapsedSeconds} seconds");

            }
        }
        private static async Task ProcessCampusAndTerm(string campusName, string termName)
        {
            // Create initial context and page for getting subject list

            if (_playwright == null || _browser == null)
            {
                throw new InvalidOperationException("Playwright or browser is not initialized.");
            }
            await using var mainContext = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                JavaScriptEnabled = true
            });

            var mainPage = await mainContext.NewPageAsync();

            try
            {
                // Navigate to main page and select campus/term
                var subjectData = await GetSubjectList(mainPage, campusName, termName);

                if (subjectData.Count == 0)
                {
                    Console.WriteLine($"No subjects found for {campusName} {termName}");
                    return;
                }

                Console.WriteLine($"Found {subjectData.Count} subjects for {campusName} {termName}");

                // Track subjects that we successfully process
                var processedSubjects = new List<string>();

                // Process in batches with parallelization
                int maxParallel = Math.Min(20, subjectData.Count); // Limit parallelism
                var batches = SplitIntoBatches(subjectData, maxParallel);
		int batchindexcount=0;
                var results = new ConcurrentDictionary<string, List<CourseData>>();

                await Parallel.ForEachAsync(batches, new ParallelOptions { MaxDegreeOfParallelism = maxParallel },
                    async (batch, token) =>
                    {
                        // Track batch progress
                        int batchIndex = batches.IndexOf(batch);
                        Console.WriteLine($"Starting batch {batchIndex + 1} with {batch.Count} subjects");

                        foreach (var subject in batch)
                        {
                            try
                            {
                                // IMPORTANT: Always start from the main page for each subject
                                await using var subjectContext = await _browser.NewContextAsync();
                                var subjectPage = await subjectContext.NewPageAsync();

                                // Navigate to main page and select campus/term
                                await NavigateToCampusAndTerm(subjectPage, campusName, termName);

                                // Find and click the subject
                                var success = await ClickSubject(subjectPage, subject.Name.Trim());

                                if (!success)
                                {
                                    Console.WriteLine($"Failed to click subject: {subject.Name}");
                                    continue;
                                }

                                // Wait for course data to load
                                await subjectPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
                                await Task.Delay(3000);

                                // Process course data
                                var courseData = await ProcessCourseData(subjectPage, subject.Name.Trim(), subject.Title.Trim());

                                if (courseData.Count > 0)
                                {
                                    results.AddOrUpdate(subject.Name.Trim(), courseData, (key, existing) =>
                                    {
                                        existing.AddRange(courseData);
                                        return existing;
                                    });

                                    lock (processedSubjects)
                                    {
                                        processedSubjects.Add(subject.Name.Trim());
                                    }
                                    //Console.WriteLine($"Processed subject: {subject.Name} with {courseData.Count} courses");
                                    
                                }
                                else
                                {
                                    Console.WriteLine($"No courses found for subject: {subject.Name}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing subject {subject.Name}: {ex.Message}");
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Completed batch {batchIndex + 1}");
                        batchindexcount++;
                        Console.WriteLine($" percentage complete: {((double)(batchindexcount) / batches.Count) * 100}%");
                        Console.ResetColor();
                    });

                // Log missing subjects
                var missingSubjects = subjectData.Select(s => s.Name.Trim())
                                               .Except(processedSubjects)
                                               .ToList();

                if (missingSubjects.Count > 0)
                {
                    Console.WriteLine($"Missing subjects: {string.Join(", ", missingSubjects)}");
                    foreach (var subject in missingSubjects)
                    {
                        try
                        {
                            Console.WriteLine(
                            $"Reprocessing subject {subject} for {campusName} {termName}");
                            await using var subjectContext = await _browser.NewContextAsync();
                            var subjectPage = await subjectContext.NewPageAsync();
                            await NavigateToCampusAndTerm(subjectPage, campusName, termName);
                            var success = await ClickSubject(subjectPage, subject.Trim());
                            if (!success)
                            {
                                Console.WriteLine($"Failed to click subject: {subject}");
                                continue;
                            }
                            await subjectPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
                            await Task.Delay(3000);
                            var courseData = await ProcessCourseData(subjectPage, subject.Trim(), subject.Trim());
                            if (courseData.Count > 0)
                            {
                                results.AddOrUpdate(subject.Trim(), courseData, (key, existing) =>
                                {
                                    existing.AddRange(courseData);
                                    return existing;
                                });
                                processedSubjects.Add(subject.Trim());
                                Console.WriteLine($"Reprocessed subject: {subject} with {courseData.Count} courses");
                            }
                            else
                            {
                                Console.WriteLine($"No courses found for subject: {subject}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reprocessing subject {subject}: {ex.Message}");

                        }
                    }

                }

                // Save all results
                var allScrapedCourses = results.Values.SelectMany(courses => courses).ToList();
                AddCourseData(campusName, termName, termName, allScrapedCourses);

                //Console.WriteLine($"Completed processing {campusName} {termName} with {allScrapedCourses.Count} courses from {results.Count} subjects");
                //Console.WriteLine("\n========== DETAILED COURSE INFORMATION ==========\n");

                foreach (var kvp in results.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
                {
                   // Console.WriteLine($"\n===========================================================  Subject: {kvp.Key}  ===========================================================");

                    foreach (var course in kvp.Value.OrderBy(c => c.CourseName))
                    {
                       // Console.WriteLine($"{course.CourseName}");

                        foreach (var section in course.Sections.OrderBy(s => s.SectionNumber))
                        {
                            foreach (var detail in section.CourseDescriptionDetails)
                            {
                                int maxWidth = 80;
                                string wrappedDescription = WrapText(detail.CourseDescription, maxWidth);
                               // Console.WriteLine($"        *Course Description: {detail.CourseDescription}, Course PreRecs: {detail.CoursePrerequisite}, Credits: {detail.CourseCredit}");
                               // Console.WriteLine($"        *Special Fee: {detail.SpecialCourseFee}, Consent: {detail.ConsentRequired}, Crosslisted: {detail.CrosslistedCourses}, Conjoint: {detail.ConjointCourses}");
                                //Console.WriteLine($"        *UCORE: {detail.UCORE}, , GER: {detail.GERCode}, Writing: {detail.WritingInTheMajor}");
                                //Console.WriteLine($"        *Cooperative: {detail.Cooperative}, Instr. Mode: {detail.InstructionMode}");
                               // Console.WriteLine($"        *Comment: {detail.Comment}, Footnotes: {detail.Footnotes}");
                                foreach( var instructor in detail.Instructors)
                                {
                                   // Console.WriteLine($"        *Instructor: {instructor}");
                                }
                                foreach (var m in detail.Meetings)
                                {
                                    //Console.WriteLine($"    Days: {m.Days}, Time: {m.Time},Location: {m.Location}");
                                }
                            }
                        }
                    }
                }

                //Console.WriteLine("\n========== END DETAILED COURSE INFORMATION ==========\n");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {campusName} {termName}: {ex.Message}");
            }
        }

        private static List<List<(string Name, string Title)>> SplitIntoBatches(List<(string Name, string Title)> subjects, int batchCount)
        {
            var batches = new List<List<(string Name, string Title)>>();
            int batchSize = (int)Math.Ceiling((double)subjects.Count / batchCount);

            for (int i = 0; i < batchCount; i++)
            {
                int startIndex = i * batchSize;
                int count = Math.Min(batchSize, subjects.Count - startIndex);

                if (count <= 0) break;

                batches.Add(subjects.GetRange(startIndex, count));
            }

            return batches;
        }

        private static async Task<List<(string Name, string Title)>> GetSubjectList(IPage page, string campusName, string termName)
        {
            var subjects = new List<(string Name, string Title)>();

            try
            {
                // Navigate to campus and term
                await NavigateToCampusAndTerm(page, campusName, termName);

                // Extract subject list
                var subjectLinks = await page.QuerySelectorAllAsync("td.zebratablesubject a");
                if (subjectLinks.Count == 0)
                {
                    // Try alternative selector
                    subjectLinks = await page.QuerySelectorAllAsync("tr.zebratable a");
                }

                foreach (var link in subjectLinks)
                {
                    var subjectName = await link.TextContentAsync();

                    // Find the title cell
                    var row = await link.EvaluateHandleAsync("el => el.closest('tr')");
                    var titleCell = await row.AsElement().QuerySelectorAsync("td.zebratabletitle");
                    var titleText = "";

                    if (titleCell != null)
                    {
                        titleText = await titleCell.TextContentAsync();
                    }

                    subjects.Add((subjectName.Trim(), titleText.Trim()));
                }

                Console.WriteLine($"Found {subjects.Count} subjects for {campusName} {termName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting subject list for {campusName} {termName}: {ex.Message}");
            }

            return subjects;
        }

        private static async Task NavigateToCampusAndTerm(IPage page, string campusName, string termName)
        {
            // Start at the main schedules page
           // Console.WriteLine($"Navigating to main schedules page for {campusName} {termName}...");
            await page.GotoAsync("https://schedules.wsu.edu", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 60000
            });

            // Wait for page to fully load
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(3000);

            // Look for and click the campus/term link
            var success = await page.EvaluateAsync<bool>($@"
        () => {{
            const headers = Array.from(document.querySelectorAll('.header_wrapper'));
            for (const header of headers) {{
                const cityElement = header.querySelector('.City');
                if (cityElement && cityElement.textContent.trim() === '{campusName}') {{
                    // Found the campus, now find the term link
                    const termLinks = Array.from(header.querySelectorAll('.nav-item'));
                    for (const link of termLinks) {{
                        if (link.textContent.includes('{termName}')) {{
                            const anchor = link.querySelector('a');
                            if (anchor) {{
                                // Scroll into view and click
                                anchor.scrollIntoView({{block: 'center'}});
                                setTimeout(() => {{ anchor.click(); }}, 500);
                                return true;
                            }}
                        }}
                    }}
                }}
            }}
            return false;
        }}
    ");

            if (!success)
            {
                throw new Exception($"Failed to navigate to {campusName} {termName}");
            }

            // Wait for navigation
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(5000); // Give extra time for the page to stabilize

            //Console.WriteLine($"Successfully navigated to {campusName} {termName}");
        }

        private static async Task<bool> ClickSubject(IPage page, string subjectName)
        {
            try
            {
                var success = await page.EvaluateAsync<bool>($@"
            () => {{
                const links = Array.from(document.querySelectorAll('td.zebratablesubject a, tr.zebratable a'));
                for (const link of links) {{
                    if (link.textContent.trim() === '{subjectName}') {{
                        link.scrollIntoView({{block: 'center'}});
                        setTimeout(() => {{ link.click(); }}, 500);
                        return true;
                    }}
                }}
                return false;
            }}
        ");

                if (success)
                {
                    // Wait for navigation and content to load
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    await Task.Delay(3000);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clicking subject {subjectName}: {ex.Message}");
                return false;
            }
        }

        private static async Task<List<CourseData>> ProcessCourseData(IPage page, string degree, string title)
        {
            List<CourseData> courses = new List<CourseData>();
            Dictionary<string, SectionData> sectionMap = new Dictionary<string, SectionData>();

            try
            {
                // Check if rows exist
                var rowsExist = await page.QuerySelectorAsync("table tbody tr") != null;
                if (!rowsExist)
                {
                    Console.WriteLine($"No course rows found for {degree}");
                    return courses;
                }

                // Get all rows
                var allRows = await page.QuerySelectorAllAsync("table tbody tr");
                //Console.WriteLine($"Found {allRows.Count} rows for {degree}");

                CourseData? currentCourse = null;

                // First pass: Process basic section information
                foreach (var row in allRows)
                {
                    try
                    {
                        // Check if this is a section divider or course header
                        var rowClass = await row.GetAttributeAsync("class") ?? "";

                        if (!rowClass.Contains("sectionlistdivider"))
                        {
                            // This is a course header row
                            var headerText = await row.TextContentAsync();
                            headerText = headerText.Trim();

                            if (headerText.StartsWith(degree) ||
                                (degree.Contains("_") && headerText.StartsWith(degree.Replace("_", " "))))
                            {
                                if (rowClass.Contains("comment-text"))
                                    continue;

                                currentCourse = new CourseData { CourseName = headerText, Title = title };
                                courses.Add(currentCourse);
                            }
                        }
                        else if (currentCourse != null)
                        {
                            // This is a section row
                            var cells = await row.QuerySelectorAllAsync("td");
                            if (cells.Count < 5) continue;

                            // Extract cell data safely with fallbacks
                            string GetCellText(IElementHandle cell, string defaultValue = "")
                            {
                                try
                                {
                                    return cell.TextContentAsync().GetAwaiter().GetResult().Trim();
                                }
                                catch
                                {
                                    return defaultValue;
                                }
                            }

                            var secCell = cells.FirstOrDefault(c => c.GetAttributeAsync("class").GetAwaiter().GetResult() == "sched_sec");
                            var slnCell = cells.FirstOrDefault(c => c.GetAttributeAsync("class").GetAwaiter().GetResult() == "sched_sln");
                            var limitCell = cells.FirstOrDefault(c => c.GetAttributeAsync("class").GetAwaiter().GetResult() == "sched_limit");
                            var enrlCell = cells.FirstOrDefault(c => c.GetAttributeAsync("class").GetAwaiter().GetResult() == "sched_enrl");
                            var crCell = cells.FirstOrDefault(c => c.GetAttributeAsync("class").GetAwaiter().GetResult() == "sched_cr");
                            var daysCell = cells.FirstOrDefault(c => c.GetAttributeAsync("class").GetAwaiter().GetResult() == "sched_days");
                            var locCell = cells.FirstOrDefault(c => c.GetAttributeAsync("class").GetAwaiter().GetResult() == "sched_loc");
                            var instructorCell = cells.FirstOrDefault(c => c.GetAttributeAsync("class").GetAwaiter().GetResult() == "sched_instructor");

                            if (secCell == null || slnCell == null) continue;

                            var sectionText = GetCellText(secCell);
                            var classNumberText = GetCellText(slnCell);
                            var maxEnrolledText = limitCell != null ? GetCellText(limitCell, "0") : "0";
                            var enrolledText = enrlCell != null ? GetCellText(enrlCell, "0") : "0";
                            var credit = crCell != null ? GetCellText(crCell) : "";
                            var fullText = daysCell != null ? GetCellText(daysCell) : "";
                            var location = locCell != null ? GetCellText(locCell) : "";
                            var instructor = instructorCell != null ? GetCellText(instructorCell) : "";

                            // Process enrollment
                            int.TryParse(classNumberText, out int classNumberInt);
                            int.TryParse(maxEnrolledText, out int maxEnrolledInt);
                            int.TryParse(enrolledText, out int enrolledInt);

                            int spotsLeft = maxEnrolledInt - enrolledInt;
                            string status = "";

                            if (spotsLeft > 0)
                                status = "Open";
                            else if (spotsLeft == 0)
                                status = "Full";
                            else
                            {
                                spotsLeft = Math.Abs(spotsLeft);
                                status = "Waitlist";
                            }

                            if (!sectionText.Contains("Lab"))
                                sectionText += "    ";

                            classNumberText = classNumberInt.ToString().PadLeft(5, '0');

                            // Process days and time
                            string daysText = "";
                            string timeText = "";

                            if (string.IsNullOrWhiteSpace(fullText) || fullText.Equals("ARRGT", StringComparison.OrdinalIgnoreCase))
                            {
                                daysText = "ARRGT";
                                timeText = "";
                            }
                            else if (fullText.Contains(' '))
                            {
                                int spaceIndex = fullText.IndexOf(' ');
                                daysText = fullText.Substring(0, spaceIndex);
                                timeText = fullText.Substring(spaceIndex + 1);
                            }
                            else
                            {
                                daysText = fullText;
                                timeText = "";
                            }

                            // Create section data
                            SectionData section = new SectionData()
                            {
                                SectionNumber = sectionText,
                                Credits = credit,
                                ClassNumber = classNumberText,
                                SpotsLeft = spotsLeft,
                                Status = status,
                                Days = daysText,
                                Time = timeText,
                                Location = location,
                                Instructor = instructor
                            };

                            // Store in map and add to current course
                            sectionMap[classNumberText] = section;
                            currentCourse.Sections.Add(section);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing row: {ex.Message}");
                    }
                }

                //Console.WriteLine($"Getting detailed information for {sectionMap.Count} sections");

                foreach (var classNumber in sectionMap.Keys.ToList())
                {
                    try
                    {
                        // Find the section row that contains this class number
                        var sectionCell = await page.QuerySelectorAsync($"td.sched_sln:text-is(\"{classNumber}\")");
                        if (sectionCell == null)
                        {
                            Console.WriteLine($"Could not find section cell for class number {classNumber} in course {title}");

                            continue;
                        }

                        // Find the parent row and then the section link
                        var row = await sectionCell.EvaluateHandleAsync("el => el.closest('tr')");
                        var secCell = await row.AsElement().QuerySelectorAsync("td.sched_sec");
                        var secLink = await secCell.QuerySelectorAsync("a");

                        if (secLink == null)
                        {
                            Console.WriteLine($"Could not find section link for class number {classNumber}");
                            continue;
                        }

                        // Click the section link to view details
                       // Console.WriteLine($"Clicking section link for class number {classNumber} class {title}");

                        // 1) click with Playwrightís API
                        await secLink.ClickAsync(new ElementHandleClickOptions());

                        // 2) wait *explicitly* for the Knockout <dl> to render
                        await page.WaitForSelectorAsync("dl.pagesdl", new PageWaitForSelectorOptions
                        {
                            State = WaitForSelectorState.Visible,
                            Timeout = 10_000
                        });

                        // 3) now extract your details
                        var details = await LoadCourseDescriptionDetails(page, classNumber);

                        // Add details to the section
                        if (sectionMap.ContainsKey(classNumber))
                        {
                            await Task.Delay(3000);
                            sectionMap[classNumber].CourseDescriptionDetails.Add(details);
                                //Console.WriteLine($"Added course details for class number {classNumber} class {title}");

                            
                        }

                        // Go back to the course list
                        await page.GoBackAsync();
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                        await Task.Delay(3000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error getting details for section {classNumber}: {ex.Message}");

                        // Try to navigate back in case of error
                        try
                        {
                            await page.GoBackAsync();
                            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                            await Task.Delay(3000);
                        }
                        catch { }
                    }
                }

                return courses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProcessCourseData: {ex.Message}");
                return courses;
            }
        }

        private static async Task<CourseDescriptionDetails> LoadCourseDescriptionDetails(IPage page, string classNumber)
        {
            CourseDescriptionDetails details = new CourseDescriptionDetails();
            //Console.WriteLine($"Loading course description details for class number {classNumber}");
            const int maxAttempts = 3;
            const int retryDelayMs = 2000;
            try
            {
                // Wait for details to load
                await Task.Delay(3000);

                // Get meeting items
                var meetingItems = await page.QuerySelectorAllAsync("li.sectionmeetingitem");
                foreach (var item in meetingItems)
                {
                    var dtText = await item.QuerySelectorAsync("span.sectionmeetingspanitem:nth-child(1)");
                    var locText = await item.QuerySelectorAsync("span.sectionmeetingspanitem:nth-child(2)");

                    if (dtText == null || locText == null) continue;

                    string dtContent = await dtText.TextContentAsync();
                    string locContent = await locText.TextContentAsync();

                    var parts = dtContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var daysPart = parts.ElementAtOrDefault(0) ?? string.Empty;
                    var timeRaw = parts.ElementAtOrDefault(1) ?? string.Empty;

                    string start12 = string.Empty, end12 = string.Empty;
                    if (timeRaw.Contains('-'))
                    {
                        var timeParts = timeRaw.Split('-');
                        var t0 = timeParts[0].Trim().Replace('.', ':');
                        var t1 = timeParts[1].Trim().Replace('.', ':');

                        // Handle AM/PM designation
                        if (!t0.Contains("AM", StringComparison.OrdinalIgnoreCase) &&
                            !t0.Contains("PM", StringComparison.OrdinalIgnoreCase))
                        {
                            double hour;
                            if (double.TryParse(t0, out hour))
                            {
                                t0 = hour < 12 ? $"{t0} AM" : $"{t0} PM";
                            }
                        }

                        if (!t1.Contains("AM", StringComparison.OrdinalIgnoreCase) &&
                            !t1.Contains("PM", StringComparison.OrdinalIgnoreCase))
                        {
                            double hour;
                            if (double.TryParse(t1.Split(':')[0], out hour))
                            {
                                t1 = hour < 12 ? $"{t1} AM" : $"{t1} PM";
                            }
                        }

                        // Parse times
                        DateTime startTime, endTime;
                        if (DateTime.TryParse(t0, out startTime) && DateTime.TryParse(t1, out endTime))
                        {
                            start12 = startTime.ToString("h:mm tt");
                            end12 = endTime.ToString("h:mm tt");
                        }
                    }

                    // Add meeting info
                    details.Meetings.Add(new MeetingsInfo
                    {
                        Days = daysPart,
                        Time = string.IsNullOrEmpty(start12) ? string.Empty : $"{start12} - {end12}",
                        Location = locContent ?? string.Empty

                    });
                }

                // 2) Retry loop for the <dl> + <dt> parsing
                IElementHandle? dl = null;
                IReadOnlyList<IElementHandle> dtElements = Array.Empty<IElementHandle>();

                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    // wait up to retryDelayMs for the <dl.pagesdl> to attach
                    dl = await page.WaitForSelectorAsync("dl.pagesdl", new PageWaitForSelectorOptions
                    {
                        State = WaitForSelectorState.Attached,
                        Timeout = retryDelayMs
                    });

                    if (dl != null)
                    {
                        dtElements = await dl.QuerySelectorAllAsync("dt");
                        if (dtElements.Count > 0)
                            break;  // success!
                    }

                    Console.WriteLine($"Attempt {attempt}/{maxAttempts}: no <dt> found yetóretrying in {retryDelayMs}ms...");
                    await Task.Delay(retryDelayMs);
                }

                if (dl == null || dtElements.Count == 0)
                {
                    Console.WriteLine($" Failed to load detail <dt> tags for SLN {classNumber}. Skipping.");
                    return details;
                }

                // 3) Parse each dt ? dd pair
                foreach (var dt in dtElements)
                {
                    var key = (await dt.TextContentAsync())?.Trim() ?? "";
                    var ddHandle = await dt.EvaluateHandleAsync("d => d.nextElementSibling");
                    var val = ddHandle != null
                                 ? (await ddHandle.AsElement().TextContentAsync())?.Trim() ?? ""
                                 : "";

                    switch (key)
                    {
                        case "Course Description":
                            details.CourseDescription = val;
                            break;
                        case "Course Prerequisite":
                            details.CoursePrerequisite = val;
                            break;
                        case "Course Credit":
                            details.CourseCredit = val;
                            break;
                        case "Special Course Fee":
                            details.SpecialCourseFee = val;
                            break;
                        case "Consent required:":
                            details.ConsentRequired = val;
                            break;
                        case "Crosslisted Courses":
                            details.CrosslistedCourses = val;
                            break;
                        case "Conjoint Courses":
                            details.ConjointCourses = val;
                            break;
                        case "UCORE":
                            details.UCORE = val;
                            break;
                        case "[GRADCAPS] Graduate Capstone":
                            details.GraduateCapstone = val;
                            break;
                        case "GER Code":
                            details.GERCode = val;
                            break;
                        case "Writing in the Major":
                            details.WritingInTheMajor = val;
                            break;
                        case "Cooperative":
                            details.Cooperative = val;
                            break;
                        case "Instructor(s)":
                            // Log for debugging
                          //  Console.WriteLine($"Processing instructors for class {classNumber}");

                            // Find the dd element that contains the instructor list by looking for the correct dt first
                            var instructorDt = await page.QuerySelectorAsync("dt:has-text('Instructor(s)')");
                            if (instructorDt != null)
                            {
                                // Get the next sibling which is the dd element containing the instructor list
                                var instructorDd = await instructorDt.EvaluateHandleAsync("dt => dt.nextElementSibling");
                                if (instructorDd != null)
                                {
                                    // Now get all visible li elements within this dd
                                    var liElements = await instructorDd.AsElement().QuerySelectorAllAsync("li");

                                    // Log how many elements we found
                                 //   Console.WriteLine($"Found {liElements.Count/2} instructor elements for class {classNumber}");

                                    // Create a set to track unique instructor names
                                    HashSet<string> uniqueInstructors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                                    foreach (var li in liElements)
                                    {
                                        // Check if element is visible
                                        var isVisible = await li.EvaluateAsync<bool>(@"
                    el => {
                        const style = window.getComputedStyle(el);
                        return style.display !== 'none' && style.visibility !== 'hidden';
                    }
                ");

                                        if (!isVisible) continue;

                                        // Get name and check if it's primary
                                        var name = await li.TextContentAsync();
                                        name = name?.Trim();

                                        if (string.IsNullOrWhiteSpace(name) || name.Equals("None listed", StringComparison.OrdinalIgnoreCase))
                                            continue;

                                        // Skip duplicates
                                        if (uniqueInstructors.Contains(name))
                                            continue;

                                        uniqueInstructors.Add(name);

                                        // Check if primary
                                        var isPrimary = await li.EvaluateAsync<bool>(@"
                    el => {
                        const style = window.getComputedStyle(el);
                        const fontWeight = style.fontWeight;
                        const isBold = fontWeight === 'bold' || fontWeight === '700' || parseInt(fontWeight) >= 700;
                        const dataBind = el.getAttribute('data-bind') || '';
                        return isBold || (dataBind.includes('isPrimary()') && !dataBind.includes('!isPrimary()'));
                    }
                ");
                                        if(liElements.Count==0)
                                        {
                                            details.Instructors.Add("None listed");
                                        }

                                        // Add to collection and log
                                        details.Instructors.Add(name);

                                        if (isPrimary)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;
                                          //  Console.WriteLine($"Primary Instructor: {name}");
                                            Console.ResetColor();
                                        }
                                        else if(!isPrimary)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                         //   Console.WriteLine($"Secondary Instructor: {name}");
                                            Console.ResetColor();
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Instructor not listed");
                                        }
                                    }
                                }
                            }
                            break;
                        case "Instruction Mode":
                            details.InstructionMode = val;
                            break;
                        case "Enrollment Limit":
                            details.EnrollmentLimit = val;
                            break;
                        case "Current Enrollment":
                            details.CurrentEnrollment = val;
                            break;
                        case "Comment":
                            details.Comment = val;
                            break;
                        case "Start Date":
                            details.StartDate = val;
                            break;
                        case "End Date":
                            details.EndDate = val;
                            break;
                        case "Footnotes":
                            details.Footnotes = val;
                            break;

                    }
                }
            }
            
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadCourseDescriptionDetails: {ex.Message}");
            }

            return details;
        }

        public static void AddCourseData(string campusName, string termCode, string termDescription, List<CourseData> scrapedCourses)
        {
            Campus? campus = CampusesList.FirstOrDefault(c =>
                c.Name.Equals(campusName, StringComparison.OrdinalIgnoreCase));
            if (campus == null)
            {
                campus = new Campus { Id = CampusesList.Count + 1, Name = campusName };
                CampusesList.Add(campus);
            }

            Term? term = campus.Terms.FirstOrDefault(t =>
                t.Description.Equals(termDescription, StringComparison.OrdinalIgnoreCase));
            if (term == null)
            {
                term = new Term { Code = termCode, Description = termDescription };
                campus.Terms.Add(term);
            }

            term.Courses.AddRange(scrapedCourses);
        }

        public static string WrapText(string text, int maxLineWidth)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var words = text.Split(' ');
            var sb = new StringBuilder();

            int currentLineLength = 0;
            foreach (var word in words)
            {
                // If adding this word would exceed the maximum line length, insert a newline.
                if (currentLineLength + word.Length + 1 > maxLineWidth)
                {
                    sb.AppendLine(); // Add a newline
                    currentLineLength = 0; // Reset, accounting for the indentation length
                }
                sb.Append(word + " ");
                currentLineLength += word.Length + 1;
            }
            return sb.ToString();
        }

        public static string GetMeetingsInfoSummary(CourseDescriptionDetails details)
            => details.Meetings.Any()
                ? string.Join(", ", details.Meetings)
                : string.Empty;

        public static List<string> ConvertDetailsToStringList(CourseDescriptionDetails details)
        {
            var list = new List<string>();

            if (!string.IsNullOrWhiteSpace(details.CourseDescription))
                list.Add(details.CourseDescription);
            if (!string.IsNullOrWhiteSpace(details.CoursePrerequisite))
                list.Add(details.CoursePrerequisite);
            if (!string.IsNullOrWhiteSpace(details.CourseCredit))
                list.Add(details.CourseCredit);
            if (!string.IsNullOrWhiteSpace(details.SpecialCourseFee))
                list.Add(details.SpecialCourseFee);
            if (!string.IsNullOrWhiteSpace(details.ConsentRequired))
                list.Add(details.ConsentRequired);
            if (!string.IsNullOrWhiteSpace(details.CrosslistedCourses))
                list.Add(details.CrosslistedCourses);
            if (!string.IsNullOrWhiteSpace(details.ConjointCourses))
                list.Add(details.ConjointCourses);
            if (!string.IsNullOrWhiteSpace(details.UCORE))
                list.Add(details.UCORE);
            if (!string.IsNullOrWhiteSpace(details.GraduateCapstone))
                list.Add(details.GraduateCapstone);
            if (!string.IsNullOrWhiteSpace(details.GERCode))
                list.Add(details.GERCode);
            if (!string.IsNullOrWhiteSpace(details.WritingInTheMajor))
                list.Add(details.WritingInTheMajor);
            if (!string.IsNullOrWhiteSpace(details.Cooperative))
                list.Add(details.Cooperative);
            if (!string.IsNullOrWhiteSpace(details.InstructionMode))
                list.Add(details.InstructionMode);
            if (!string.IsNullOrWhiteSpace(details.EnrollmentLimit))
                list.Add(details.EnrollmentLimit);
            if (!string.IsNullOrWhiteSpace(details.CurrentEnrollment))
                list.Add(details.CurrentEnrollment);
            if (!string.IsNullOrWhiteSpace(details.Comment))
                list.Add(details.Comment);
            if (!string.IsNullOrWhiteSpace(details.StartDate))
                list.Add(details.StartDate);
            if (!string.IsNullOrWhiteSpace(details.EndDate))
                list.Add(details.EndDate);
            if (!string.IsNullOrWhiteSpace(details.Footnotes))
                list.Add(details.Footnotes);
            if (details.Instructors.Any())
                list.AddRange(details.Instructors);


            return list;
        }
    }

}
       
      




