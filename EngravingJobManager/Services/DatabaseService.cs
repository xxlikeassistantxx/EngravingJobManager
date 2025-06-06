using SQLite;
using EngravingJobManager.Models; // For Job, PhotoItem, SortOrderOption
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq; // Required for OrderBy, ThenBy

// Ensure this namespace is correct and matches where you expect the file to be.
// If your Services folder has a different structure, adjust accordingly.
namespace EngravingJobManager.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _initialized = false;

        public DatabaseService()
        {
            // Path will be set in Init
        }

        private async Task Init()
        {
            if (_initialized)
                return;

            if (_database == null)
            {
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "EngravingJobs.db3");
                _database = new SQLiteAsyncConnection(databasePath);
            }

            await _database.CreateTableAsync<Job>();
            await _database.CreateTableAsync<PhotoItem>();

            _initialized = true;
        }

        // --- Job Methods ---

        public async Task<int> SaveJobAsync(Job job)
        {
            await Init();
            if (job.Id != 0)
            {
                // Update existing job
                await _database.UpdateAsync(job);

                if (job.Photos != null)
                {
                    foreach (var photo in job.Photos)
                    {
                        if (photo.Id == 0 && photo.JobId == 0)
                        {
                            photo.JobId = job.Id;
                            await _database.InsertAsync(photo);
                        }
                    }
                }
                return job.Id;
            }
            else
            {
                // Insert new job
                await _database.InsertAsync(job);

                if (job.Photos != null)
                {
                    foreach (var photo in job.Photos)
                    {
                        photo.JobId = job.Id;
                        await _database.InsertAsync(photo);
                    }
                }
                return job.Id;
            }
        }

        public async Task<List<Job>> GetJobsAsync(SortOrderOption sortOrder = SortOrderOption.NewestFirst)
        {
            await Init();

            var query = _database.Table<Job>().Where(j => !j.IsDeleted);

            switch (sortOrder)
            {
                case SortOrderOption.ByJobTitle:
                    query = query.OrderBy(j => j.Title);
                    break;
                case SortOrderOption.ByClientele:
                    query = query.OrderBy(j => j.CustomerName).ThenBy(j => j.CustomerOrganization);
                    break;
                case SortOrderOption.NewestFirst:
                default:
                    query = query.OrderByDescending(j => j.DateCreated);
                    break;
            }

            var jobs = await query.ToListAsync();
            foreach (var job in jobs)
            {
                job.Photos = await GetPhotosForJobAsync(job.Id);
            }
            return jobs;
        }

        public async Task<Job> GetJobAsync(int id)
        {
            await Init();
            var job = await _database.Table<Job>().Where(j => j.Id == id).FirstOrDefaultAsync();
            if (job != null)
            {
                job.Photos = await GetPhotosForJobAsync(job.Id);
            }
            return job;
        }

        public async Task<int> DeleteJobAsync(Job job) // Soft delete
        {
            await Init();
            job.IsDeleted = true;
            return await _database.UpdateAsync(job);
        }

        public async Task<int> HardDeleteJobAsync(Job job) // Permanent delete
        {
            await Init();
            var photos = await GetPhotosForJobAsync(job.Id);
            foreach (var photo in photos)
            {
                await _database.DeleteAsync(photo);
                // if (File.Exists(photo.PhotoPath))
                // {
                //     try { File.Delete(photo.PhotoPath); }
                //     catch { /* Log or handle error */ }
                // }
            }
            return await _database.DeleteAsync(job);
        }

        // --- PhotoItem Methods ---

        public async Task<List<PhotoItem>> GetPhotosForJobAsync(int jobId)
        {
            await Init();
            return await _database.Table<PhotoItem>().Where(p => p.JobId == jobId).ToListAsync();
        }

        public async Task<int> SavePhotoItemAsync(PhotoItem photoItem)
        {
            await Init();
            if (photoItem.Id != 0)
            {
                return await _database.UpdateAsync(photoItem);
            }
            else
            {
                return await _database.InsertAsync(photoItem);
            }
        }

        public async Task<int> DeletePhotoItemAsync(PhotoItem photoItem)
        {
            await Init();
            // if (File.Exists(photoItem.PhotoPath))
            // {
            //     try { File.Delete(photoItem.PhotoPath); }
            //     catch { /* Log or handle error */ }
            // }
            return await _database.DeleteAsync(photoItem);
        }

        public async Task<List<Job>> GetDeletedJobsAsync(SortOrderOption sortOrder = SortOrderOption.NewestFirst)
        {
            await Init();

            var query = _database.Table<Job>().Where(j => j.IsDeleted);

            switch (sortOrder)
            {
                case SortOrderOption.ByJobTitle:
                    query = query.OrderBy(j => j.Title);
                    break;
                case SortOrderOption.ByClientele:
                    query = query.OrderBy(j => j.CustomerName).ThenBy(j => j.CustomerOrganization);
                    break;
                case SortOrderOption.NewestFirst:
                default:
                    query = query.OrderByDescending(j => j.DateCreated);
                    break;
            }

            var jobs = await query.ToListAsync();
            foreach (var job in jobs)
            {
                job.Photos = await GetPhotosForJobAsync(job.Id);
            }
            return jobs;
        }

        public async Task<int> RestoreJobAsync(Job job)
        {
            await Init();
            job.IsDeleted = false;
            return await _database.UpdateAsync(job);
        }
        public async Task<List<Job>> SearchJobsByTitleAsync(string searchTerm, SortOrderOption sortOrder = SortOrderOption.NewestFirst)
        {
            await Init();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // If search term is empty, return all non-deleted jobs, or an empty list.
                // Let's return an empty list for live search to avoid loading all jobs initially.
                // If you want to show all jobs when search is empty, call GetJobsAsync(sortOrder).
                return new List<Job>();
            }

            // Basic query for non-deleted jobs and title matching (case-insensitive)
            var query = _database.Table<Job>()
                                 .Where(j => !j.IsDeleted && j.Title.ToLower().Contains(searchTerm.ToLower()));

            // Apply sorting
            switch (sortOrder)
            {
                case SortOrderOption.ByJobTitle:
                    query = query.OrderBy(j => j.Title);
                    break;
                case SortOrderOption.ByClientele:
                    query = query.OrderBy(j => j.CustomerName).ThenBy(j => j.CustomerOrganization);
                    break;
                case SortOrderOption.NewestFirst:
                default:
                    query = query.OrderByDescending(j => j.DateCreated);
                    break;
            }

            var jobs = await query.ToListAsync();
            foreach (var job in jobs)
            {
                job.Photos = await GetPhotosForJobAsync(job.Id);
            }
            return jobs;
        }
        public async Task<List<Job>> SearchJobsByDateRangeAsync(DateTime startDate, DateTime endDate, SortOrderOption sortOrder = SortOrderOption.NewestFirst)
        {
            await Init();

            // Adjust endDate to include the entire day
            DateTime actualEndDate = endDate.Date.AddDays(1).AddTicks(-1); // End of the selected day

            var query = _database.Table<Job>()
                                 .Where(j => !j.IsDeleted &&
                                             j.DateCreated >= startDate.Date &&
                                             j.DateCreated <= actualEndDate);

            // Apply sorting
            switch (sortOrder)
            {
                case SortOrderOption.ByJobTitle:
                    query = query.OrderBy(j => j.Title);
                    break;
                case SortOrderOption.ByClientele:
                    query = query.OrderBy(j => j.CustomerName).ThenBy(j => j.CustomerOrganization);
                    break;
                case SortOrderOption.NewestFirst: // In this context, newest within the date range
                default:
                    query = query.OrderByDescending(j => j.DateCreated);
                    break;
            }

            var jobs = await query.ToListAsync();
            foreach (var job in jobs)
            {
                job.Photos = await GetPhotosForJobAsync(job.Id);
            }
            return jobs;
        }

        public async Task<List<Job>> SearchJobsByClienteleAsync(string searchTerm)
        {
            await Init();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Job>(); // Return empty list if search term is blank
            }

            string lowerSearchTerm = searchTerm.ToLower();

            var query = _database.Table<Job>()
                                 .Where(j => !j.IsDeleted &&
                                             (j.CustomerName.ToLower().Contains(lowerSearchTerm) ||
                                              j.CustomerOrganization.ToLower().Contains(lowerSearchTerm)))
                                 .OrderByDescending(j => j.DateCreated); // Default sort: newest first

            var jobs = await query.ToListAsync();
            foreach (var job in jobs)
            {
                job.Photos = await GetPhotosForJobAsync(job.Id);
            }
            return jobs;
        }
    }
}