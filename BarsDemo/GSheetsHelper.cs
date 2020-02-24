using BarsDemo.ConfigHelper;
using BarsDemo.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v3.Data;

namespace BarsDemo
{
    public class GSheetsHelper
    {
        #region закрытые поля
        private static readonly string[] _scopes = { SheetsService.Scope.Spreadsheets, DriveService.Scope.Drive };
        private static readonly string _applicationName = "Bars";
        private static readonly string _spreadsheetId = System.Configuration.ConfigurationManager.AppSettings["GoogleSpreadsheetId"];
        private static readonly string _email = System.Configuration.ConfigurationManager.AppSettings["email"];
        private readonly string _sheet = "Server1";
        private readonly double _hddSize = 60;
        private readonly List<DbInfo> _dbs = new List<DbInfo>();
        private static SheetsService _service;
        private static DriveService _driveService;
        #endregion

        #region статический конструктор
        static GSheetsHelper()
        {
            GoogleCredential credential;
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(_scopes);
            }

            // Создаем сервисы Google Sheets API и Google Drive API.
            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });
            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });

            if (_spreadsheetId == null) // Если в конфиге не указана таблица создадим новую
            {
                var sp = new Spreadsheet()
                {
                    Properties = new SpreadsheetProperties()
                    {
                        Title = "Bars",
                        Locale = "ru_RU",
                    },
                    Sheets = new List<Sheet>
                    {
                        new Sheet()
                        {
                            Properties = new SheetProperties()
                            {
                                SheetType = "GRID",
                                SheetId = 0,
                                Title = "Server1",
                                GridProperties = new GridProperties()
                                {
                                    RowCount = 1000,
                                    ColumnCount = 100
                                }
                            }
                        }
                    }

                };
                var spreadsheet = _service.Spreadsheets.Create(sp).Execute();
                
                _spreadsheetId = spreadsheet.SpreadsheetId;
                Console.WriteLine($"GoogleSpreadsheetId не указан в файле конфигурации, создан новый документ {spreadsheet.SpreadsheetUrl}");

                if (_email != null) // дадим доступ для аккауна связанного с email указанным в конфигурационном файле
                {
                    try
                    {
                        Console.WriteLine($"Предоставим доступ к документу пользователю ассоциированному с email {_email}");
                        _driveService.Permissions.Create(
                        new Permission()
                        {
                            Type = "user",
                            Role = "writer",
                            EmailAddress = _email
                        },
                        _spreadsheetId
                        ).Execute();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Что то пошло не так, текст ошибки  {e.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Доступ к документу с GoogleSpreadsheetId {_spreadsheetId}");
            }
        }
        #endregion

        #region конструктор
        public GSheetsHelper(string srvName, string hddSize, List<DbInfo> dbs)
        {
            _sheet = srvName;
            _hddSize = Double.Parse(hddSize); // в реальном приложении не помешала бы проверка на корректность входных данных из конигурационного файла
            _dbs = dbs;
        }
        #endregion

        private void CreateHeader()
        {
            #region индекс страницы по названию
            //получим индекс страницы по названию
            Spreadsheet spr = _service.Spreadsheets.Get(_spreadsheetId).Execute();
            Sheet sh = spr.Sheets.Where(s => s.Properties.Title == _sheet).FirstOrDefault();
            int sheetId = (int)sh.Properties.SheetId;
            #endregion

            #region вставим текст
            var range = $"{_sheet}!A:D";
            var valueRange = new ValueRange();
            var oblist = new List<object>() { "Сервер", "База данных", "Размер в ГБ", "Дата обновления" };
            valueRange.Values = new List<IList<object>> { oblist };
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = appendRequest.Execute(); // выполняем запрос
            #endregion

            #region установим формат границ ячеек
            var updateBordersRequest = new Request()
            {
                UpdateBorders = new UpdateBordersRequest()
                {
                    Range = new GridRange()
                    {
                        SheetId = sheetId,
                        StartColumnIndex = 0,
                        StartRowIndex = 0,
                        EndColumnIndex = 4,
                        EndRowIndex = 1
                    },
                    Top = new Border()
                    {
                        Width = 2,
                        Style = "solid"
                    },
                    Bottom = new Border()
                    {
                        Width = 2,
                        Style = "solid"

                    },
                    Left = new Border()
                    {
                        Width = 2,
                        Style = "solid"
                    },
                    Right = new Border()
                    {
                        Width = 2,
                        Style = "solid"
                    },
                    InnerVertical = new Border()
                    {
                        Width = 2,
                        Style = "solid"
                    },
                    InnerHorizontal = new Border()
                    {
                        Width = 2,
                        Style = "solid"
                    }
                }
            };
            #endregion

            #region установим формат шрифта
            var updateTextRequest = new Request()
            {
                RepeatCell = new RepeatCellRequest()
                {
                    Range = new GridRange()
                    {
                        SheetId = sheetId,
                        StartColumnIndex = 0,
                        StartRowIndex = 0,
                        EndColumnIndex = 4,
                        EndRowIndex = 1
                    },
                    Cell = new CellData()
                    {
                        UserEnteredFormat = new CellFormat()
                        {
                            TextFormat = new TextFormat()
                            {
                                Bold = true
                            }
                        }
                    },
                    Fields = "UserEnteredFormat(TextFormat)"
                }
            };
            #endregion

            #region Зададим ширину столбцов
            var updateDimensionPropertysRequest = new Request()
            {
                UpdateDimensionProperties = new UpdateDimensionPropertiesRequest()
                {
                    Range = new DimensionRange()
                    {
                        SheetId = sheetId,
                        Dimension = "columns",
                        StartIndex = 0,
                        EndIndex = 4
                    },
                    Properties = new DimensionProperties()
                    {
                        PixelSize = 130
                    },
                    Fields = "PixelSize"

                }
            };
            #endregion

            BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();
            bussr.Requests = new List<Request>();
            bussr.Requests.Add(updateTextRequest);
            bussr.Requests.Add(updateBordersRequest);
            bussr.Requests.Add(updateDimensionPropertysRequest);
            SpreadsheetsResource.BatchUpdateRequest bur = _service.Spreadsheets.BatchUpdate(bussr, _spreadsheetId);
            BatchUpdateSpreadsheetResponse responseUpdate = bur.Execute();
        }

        private void CreateBody()
        {
            var range = $"{_sheet}!A:D";
            var valueRange = new ValueRange();
            double allDbSize = 0;
            foreach (DbInfo db in _dbs)
            {
                allDbSize += db.DBSizeGB;
                var oblist = new List<object>() { _sheet, db.DatabaseName, db.DBSizeGB.ToString("#.###"), DateTime.Today.ToString("dd.MM.yyyy")};
                valueRange.Values = new List<IList<object>> { oblist };
                var appendRequest = _service.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendReponse = appendRequest.Execute(); 
            }
            // добавим последнюю строчку
            var oblistFooter = new List<object>() { _sheet, "Свободно", (_hddSize-allDbSize).ToString("#.###"), DateTime.Today.ToString("dd.MM.yyyy") };
            valueRange.Values = new List<IList<object>> {oblistFooter};
            var appendFooterRequest = _service.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
            appendFooterRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendFooterReponse = appendFooterRequest.Execute();
        }

        private void ClearSheet() // создает или очищает существующий sheet
        {
            Spreadsheet spr = _service.Spreadsheets.Get(_spreadsheetId).Execute();
            Sheet sh = spr.Sheets.Where(s => s.Properties.Title == _sheet).FirstOrDefault();
            if (sh == null)
            {
                var addSheetRequest = new AddSheetRequest();
                addSheetRequest.Properties = new SheetProperties();
                addSheetRequest.Properties.Title = _sheet;
                BatchUpdateSpreadsheetRequest busr = new BatchUpdateSpreadsheetRequest();
                busr.Requests = new List<Request>();
                busr.Requests.Add(new Request
                {
                    AddSheet = addSheetRequest
                });
                var batchUpdateRequest = _service.Spreadsheets.BatchUpdate(busr, _spreadsheetId);
                batchUpdateRequest.Execute();
            }
            else
            {
                ClearValuesRequest requestBody = new ClearValuesRequest();
                SpreadsheetsResource.ValuesResource.ClearRequest request = _service.Spreadsheets.Values.Clear(requestBody, _spreadsheetId, _sheet);
                ClearValuesResponse response = request.Execute();
            }
        }

        public void UpdateSheet()
        {
            ClearSheet();
            CreateHeader();
            CreateBody();
        }
    }
}
