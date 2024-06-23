using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using srtool;
using blueprint.modules.drive.response;
using blueprint.core;
using blueprint.modules.database;

namespace blueprint.modules.drive.logic
{
    public class DriveModule : Module<DriveModule>
    {
        public IMongoCollection<database.File> file { get; private set; }
        public string WebRootPath { get; set; }
        public override async Task RunAsync()
        {
            await base.RunAsync();
            file = DatabaseModule.Instance.database.GetCollection<database.File>("file");
        }

        public async Task<FileResponse> Add(string accountId, IFormFile file, string title)
        {
            if (file != null && file.Length > 0)
            {
                var dateTime = DateTime.UtcNow;
                var uploadsFolder = $"{WebRootPath}\\drive\\{dateTime.Year}\\{dateTime.Month}\\{dateTime.Day}";

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var uniqueFileName = $"{Utility.CalculateMD5Hash(Guid.NewGuid().ToString()).Substring(0, 8)}_{Utility.SlugMaker(fileName)}";

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                string extention = Path.GetExtension(file.FileName).TrimStart('.');

                using (var fileStream = new FileStream(extention != "" ? $"{filePath}.{extention}" : filePath, FileMode.Create))
                    await file.CopyToAsync(fileStream);

                var dbFile = new database.File();
                dbFile._id = ObjectId.GenerateNewId();
                dbFile.extention = extention;
                dbFile.name = fileName;
                dbFile.uniqueName = uniqueFileName;

                dbFile.title = title;
                dbFile.account_id = accountId.ToObjectId();
                dbFile.createDateTime = dateTime;
                dbFile.size = file.Length;


                await this.file.InsertOneAsync(dbFile);

                return await Get(dbFile._id.ToString());
            }

            return null;
        }
        public async Task<FileResponse> Get(string id)
        {
            var result = await List(new List<string>() { id });
            return result.FirstOrDefault();
        }
        public async Task<List<FileResponse>> List(List<string> ids)
        {
            var _ids = ids.Select(i => i.ToObjectId()).Distinct().ToList();
            //var dbFiles = await file.AsQueryable().Where(i => _ids.Contains(i._id)).ToListAsync();
            var dbFiles = await file.Find_Cahce("_id", _ids);

            var result = dbFiles.Select(i => new
            {
                i.uniqueName,
                file = new FileResponse()
                {
                    id = i._id.ToString(),
                    name = i.name,
                    extention = i.extention,
                    title = i.title,
                    dateTime = i.createDateTime,
                }
            }).ToList();

            result.ForEach(media =>
            {
                string e = null;
                if (media.file.extention != "")
                    e = "." + media.file.extention;
                else
                    e = "";

                media.file.url = $"/drive/{media.file.dateTime.Year}/{media.file.dateTime.Month}/{media.file.dateTime.Day}/{media.uniqueName}{e}";
            });

            return result.Select(i => i.file).ToList();
        }

    }
}