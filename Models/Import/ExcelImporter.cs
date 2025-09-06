using MillenniumWebFixed.Models;
using OfficeOpenXml;

namespace MillenniumWebFixed.Services
{
    public class ExcelImporter
    {
        private readonly AppDbContext db;

        public ExcelImporter(AppDbContext context)
        {
            db = context;
        }

        // Helper For Empty Rows
        private bool IsRowEmpty(ExcelWorksheet sheet, int row, params int[] requiredColumnIndexes)
        {
            foreach (var colIndex in requiredColumnIndexes)
            {
                if (!string.IsNullOrWhiteSpace(sheet.Cells[row, colIndex].Text))
                {
                    return false;
                }
            }
            return true;
        }

        //public class ImportContext
        //{
        //    public int GeneralProjectDataId { get; set; } // maps to your current "projectId"
        //    public int ProjectId { get; set; }            // future use
        //    public int ProjectVersionId { get; set; }     // future use
        //}

        // Overloads that keep your switch using ctx, with zero data logic changes
        public void Importxls_FramingZones(ExcelWorksheet sheet, ImportContext ctx) => Importxls_FramingZones(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Frames(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Frames(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Manufactureframes(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Manufactureframes(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Timber(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Timber(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Cladding(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Cladding(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Boards(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Boards(sheet, ctx.GeneralProjectDataId);
        public void Importxls_ConnectorPlates(ExcelWorksheet sheet, ImportContext ctx) => Importxls_ConnectorPlates(sheet, ctx.GeneralProjectDataId);
        public void Importxls_PosiStruts(ExcelWorksheet sheet, ImportContext ctx) => Importxls_PosiStruts(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Metalwork(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Metalwork(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Bracing(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Bracing(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Walls(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Walls(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Surfaces(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Surfaces(sheet, ctx.GeneralProjectDataId);
        public void Importxls_AtticRooms(ExcelWorksheet sheet, ImportContext ctx) => Importxls_AtticRooms(sheet, ctx.GeneralProjectDataId);
        public void Importxls_RoofingData(ExcelWorksheet sheet, ImportContext ctx) => Importxls_RoofingData(sheet, ctx.GeneralProjectDataId);
        public void Importxls_SundryItems(ExcelWorksheet sheet, ImportContext ctx) => Importxls_SundryItems(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Fasteners(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Fasteners(sheet, ctx.GeneralProjectDataId);
        public void Importxls_DoorsandWindows(ExcelWorksheet sheet, ImportContext ctx) => Importxls_DoorsandWindows(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Decking(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Decking(sheet, ctx.GeneralProjectDataId);
        public void Importxls_Sheeting(ExcelWorksheet sheet, ImportContext ctx) => Importxls_Sheeting(sheet, ctx.GeneralProjectDataId);
        public void Importxls_CO2material(ExcelWorksheet sheet, ImportContext ctx) => Importxls_CO2material(sheet, ctx.GeneralProjectDataId);



        public void Importxls_FramingZones(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_FramingZones
                {
                    GeneralProjectDataId = projectId,
                    Name = sheet.Cells[row, 1].Text,
                    Type = sheet.Cells[row, 2].Text,
                    Span_m = sheet.Cells[row, 3].Text,
                    Pitch_degree = sheet.Cells[row, 4].Text,
                    Price_R = sheet.Cells[row, 5].Text,
                    Layout = sheet.Cells[row, 6].Text,
                    Storey = sheet.Cells[row, 7].Text
                };
                db.xls_FramingZones.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Frames(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Frames
                {
                    GeneralProjectDataId = projectId,
                    Name = sheet.Cells[row, 1].Text,
                    Label = sheet.Cells[row, 2].Text,
                    Quantity_single_ply = sheet.Cells[row, 3].Text,
                    Plies = sheet.Cells[row, 4].Text,
                    Family = sheet.Cells[row, 5].Text,
                    Type = sheet.Cells[row, 6].Text,
                    Family_ID = sheet.Cells[row, 7].Text,
                    Type_ID = sheet.Cells[row, 8].Text,
                    Span_m = sheet.Cells[row, 9].Text,
                    Pitch_degree = sheet.Cells[row, 10].Text,
                    Pitch_TCL_degree = sheet.Cells[row, 11].Text,
                    Pitch_TCR_degree = sheet.Cells[row, 12].Text,
                    Center_mm = sheet.Cells[row, 13].Text,
                    Actual_cube_m3 = sheet.Cells[row, 14].Text,
                    Actual_cube_m3_Single = sheet.Cells[row, 15].Text,
                    Stock_Cube_m3 = sheet.Cells[row, 16].Text,
                    Stock_Cube_m3_Single = sheet.Cells[row, 17].Text,
                    Connector_area_dm2 = sheet.Cells[row, 18].Text,
                    Connector_area_dm2_Single = sheet.Cells[row, 19].Text,
                    Connector_points = sheet.Cells[row, 20].Text,
                    Splices = sheet.Cells[row, 21].Text,
                    Price_per_frame_R = sheet.Cells[row, 22].Text,
                    Parts = sheet.Cells[row, 23].Text,
                    Total_parts = sheet.Cells[row, 24].Text,
                    Production_case = sheet.Cells[row, 25].Text,
                    Quote_group = sheet.Cells[row, 26].Text,
                    Framing_zone = sheet.Cells[row, 27].Text,
                    Production_set = sheet.Cells[row, 28].Text,
                    Storey = sheet.Cells[row, 29].Text,
                    Layout = sheet.Cells[row, 30].Text,
                    Overall_span = sheet.Cells[row, 31].Text,
                    Overall_height = sheet.Cells[row, 32].Text,
                    Quant__roof_segments = sheet.Cells[row, 33].Text,
                    Quant__ceiling_segments = sheet.Cells[row, 34].Text,
                    Match_group = sheet.Cells[row, 35].Text,
                    Match_group_master = sheet.Cells[row, 36].Text,
                    Saw_set_up = sheet.Cells[row, 37].Text,
                    Cutting_time_instance = sheet.Cells[row, 38].Text,
                    Timber_count_single = sheet.Cells[row, 39].Text,
                    Press_set_up = sheet.Cells[row, 40].Text,
                    Pressing_time_instance = sheet.Cells[row, 41].Text,
                    Press_count_single = sheet.Cells[row, 42].Text,
                    Reductions_single = sheet.Cells[row, 43].Text,
                    Housings_single = sheet.Cells[row, 44].Text,
                    Holes_single = sheet.Cells[row, 45].Text,
                    Attic = sheet.Cells[row, 46].Text,
                    On_concrete = sheet.Cells[row, 47].Text,
                    Nbr_of_supports = sheet.Cells[row, 48].Text,
                    Has_stub_From = sheet.Cells[row, 49].Text,
                    Has_stub_To = sheet.Cells[row, 50].Text,
                    Overhang_From = sheet.Cells[row, 51].Text,
                    Overhang_To = sheet.Cells[row, 52].Text,
                    Transportation_Length = sheet.Cells[row, 53].Text,
                    Transportation_Height = sheet.Cells[row, 54].Text,
                    Frame_Part_Transportation_Sizes = sheet.Cells[row, 55].Text,
                    Weight = sheet.Cells[row, 56].Text,
                    Wall_panel_window_area = sheet.Cells[row, 57].Text,
                    Wall_panel_door_area = sheet.Cells[row, 58].Text,
                    Wall_panel_area_excl_openings = sheet.Cells[row, 59].Text,
                    Wall_panel_assembly_time_h = sheet.Cells[row, 60].Text,
                    Room_width = sheet.Cells[row, 61].Text,
                    Room_height = sheet.Cells[row, 62].Text,
                    Cassette_label = sheet.Cells[row, 63].Text
                };
                db.xls_Frames.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Manufactureframes(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Manufactureframes
                {
                    GeneralProjectDataId = projectId,
                    Name = sheet.Cells[row, 1].Text,
                    Quantity = sheet.Cells[row, 2].Text,
                    Plies = sheet.Cells[row, 3].Text,
                    Overallspanmm = sheet.Cells[row, 4].Text,
                    Overallheightm = sheet.Cells[row, 5].Text,
                    Roof_planes = sheet.Cells[row, 6].Text,
                    Actual_cube_m3 = sheet.Cells[row, 7].Text,
                    Connector_area_dm2 = sheet.Cells[row, 8].Text,
                    Connector_points = sheet.Cells[row, 9].Text,
                    Production_set = sheet.Cells[row, 10].Text,
                    Frame = sheet.Cells[row, 11].Text,
                    Habitability = sheet.Cells[row, 12].Text,
                    On_concrete = sheet.Cells[row, 13].Text,
                    Storey = sheet.Cells[row, 14].Text,
                    Layout = sheet.Cells[row, 15].Text
                };
                db.xls_Manufactureframes.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Timber(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Timber
                {
                    GeneralProjectDataId = projectId,
                    Truss = sheet.Cells[row, 1].Text,
                    Label = sheet.Cells[row, 2].Text,
                    Quantity = sheet.Cells[row, 3].Text,
                    Quantity_ply = sheet.Cells[row, 4].Text,
                    Type = sheet.Cells[row, 5].Text,
                    Overall_length_m = sheet.Cells[row, 6].Text,
                    Thickness_mm = sheet.Cells[row, 7].Text,
                    Depth_mm = sheet.Cells[row, 8].Text,
                    Grade = sheet.Cells[row, 9].Text,
                    Material_name = sheet.Cells[row, 10].Text,
                    Product_code = sheet.Cells[row, 11].Text,
                    Price_R_m3 = sheet.Cells[row, 12].Text,
                    Part_Number = sheet.Cells[row, 13].Text,
                    Member_layers = sheet.Cells[row, 14].Text
                };
                db.xls_Timber.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Cladding(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Cladding
                {
                    GeneralProjectDataId = projectId,
                    Frame = sheet.Cells[row, 1].Text,
                    Quantity = sheet.Cells[row, 2].Text,
                    Name = sheet.Cells[row, 3].Text,
                    Product_code = sheet.Cells[row, 4].Text,
                    Thickness_mm = sheet.Cells[row, 5].Text,
                    Width_mm = sheet.Cells[row, 6].Text,
                    Length_mm = sheet.Cells[row, 7].Text,
                    Weight_kg_m = sheet.Cells[row, 8].Text,
                    Price_R_m = sheet.Cells[row, 9].Text
                };
                db.xls_Cladding.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Boards(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Boards
                {
                    GeneralProjectDataId = projectId,
                    Quantity = sheet.Cells[row, 1].Text,
                    Type = sheet.Cells[row, 2].Text,
                    Board_name = sheet.Cells[row, 3].Text,
                    Product_code = sheet.Cells[row, 4].Text,
                    Thickness_mm = sheet.Cells[row, 5].Text,
                    Width_mm = sheet.Cells[row, 6].Text,
                    Length_mm = sheet.Cells[row, 7].Text,
                    Weight_kg = sheet.Cells[row, 8].Text,
                    Price_R = sheet.Cells[row, 9].Text
                };
                db.xls_Boards.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_ConnectorPlates(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_ConnectorPlates
                {
                    GeneralProjectDataId = projectId,
                    Truss = sheet.Cells[row, 1].Text,
                    Quantity = sheet.Cells[row, 2].Text,
                    Quantity_ply = sheet.Cells[row, 3].Text,
                    Depth_mm = sheet.Cells[row, 4].Text,
                    Length_mm = sheet.Cells[row, 5].Text,
                    Gauge = sheet.Cells[row, 6].Text,
                    Material_name = sheet.Cells[row, 7].Text,
                    Product_code = sheet.Cells[row, 8].Text,
                    Price_R_m2 = sheet.Cells[row, 9].Text,
                    Part_Number = sheet.Cells[row, 10].Text
                };
                db.xls_ConnectorPlates.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_PosiStruts(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_PosiStruts
                {
                    GeneralProjectDataId = projectId,
                    Frame = sheet.Cells[row, 1].Text,
                    Size = sheet.Cells[row, 2].Text,
                    Quantity_Full_V = sheet.Cells[row, 3].Text,
                    Quantity_Single = sheet.Cells[row, 4].Text,
                    Price_Full_V = sheet.Cells[row, 5].Text,
                    Price_Single = sheet.Cells[row, 6].Text
                };
                db.xls_PosiStruts.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Metalwork(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Metalwork
                {
                    GeneralProjectDataId = projectId,
                    Name = sheet.Cells[row, 1].Text,
                    Solution = sheet.Cells[row, 2].Text,
                    Fixing = sheet.Cells[row, 3].Text,
                    Bracket_qty = sheet.Cells[row, 4].Text,
                    Status = sheet.Cells[row, 5].Text,
                    Failure_reason = sheet.Cells[row, 6].Text,
                    Supplier = sheet.Cells[row, 7].Text,
                    Group = sheet.Cells[row, 8].Text,
                    Hanger = sheet.Cells[row, 9].Text,
                    Description = sheet.Cells[row, 10].Text,
                    Material_Name = sheet.Cells[row, 11].Text,
                    Width_mm = sheet.Cells[row, 12].Text,
                    Height_mm = sheet.Cells[row, 13].Text,
                    Depth_mm = sheet.Cells[row, 14].Text,
                    Flange_width_mm = sheet.Cells[row, 15].Text,
                    Quote_group = sheet.Cells[row, 16].Text,
                    Final_Price_R = sheet.Cells[row, 17].Text,
                    Fixing_1 = sheet.Cells[row, 18].Text,
                    Fixing_quantity = sheet.Cells[row, 19].Text
                };
                db.xls_Metalwork.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Bracing(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Bracing
                {
                    GeneralProjectDataId = projectId,
                    Name = sheet.Cells[row, 1].Text,
                    Type = sheet.Cells[row, 2].Text,
                    Length_m = sheet.Cells[row, 3].Text,
                    Stocklength_m = sheet.Cells[row, 4].Text,
                    Thickness_mm = sheet.Cells[row, 5].Text,
                    Depth_mm = sheet.Cells[row, 6].Text,
                    Grade = sheet.Cells[row, 7].Text,
                    Material_name = sheet.Cells[row, 8].Text,
                    Quote_group = sheet.Cells[row, 9].Text,
                    Price_R_m3 = sheet.Cells[row, 10].Text
                };
                db.xls_Bracing.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Walls(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Walls
                {
                    GeneralProjectDataId = projectId,
                    Chain = sheet.Cells[row, 1].Text,
                    Name = sheet.Cells[row, 2].Text,
                    Type = sheet.Cells[row, 3].Text,
                    Description = sheet.Cells[row, 4].Text,
                    Supporting = sheet.Cells[row, 5].Text,
                    Length_m = sheet.Cells[row, 6].Text,
                    Horizontal_length_m = sheet.Cells[row, 7].Text,
                    Wallplate_thickness_mm = sheet.Cells[row, 8].Text,
                    Wallplate_depth_mm = sheet.Cells[row, 9].Text,
                    Height_reference_mm = sheet.Cells[row, 10].Text,
                    Height_bottom_mm = sheet.Cells[row, 11].Text
                };
                db.xls_Walls.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Surfaces(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Surfaces
                {
                    GeneralProjectDataId = projectId,
                    Chain = sheet.Cells[row, 1].Text,
                    Name = sheet.Cells[row, 2].Text,
                    Type = sheet.Cells[row, 3].Text,
                    Pitch_degree = sheet.Cells[row, 4].Text,
                    Area_m2 = sheet.Cells[row, 5].Text,
                    Area_outside_walls_m2 = sheet.Cells[row, 6].Text,
                    Gable = sheet.Cells[row, 7].Text
                };
                db.xls_Surfaces.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_AtticRooms(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_AtticRooms
                {
                    GeneralProjectDataId = projectId,
                    Name = sheet.Cells[row, 1].Text,
                    Room_height_m = sheet.Cells[row, 2].Text,
                    Area_m2 = sheet.Cells[row, 3].Text,
                    Living_area_m2 = sheet.Cells[row, 4].Text,
                    Concrete_floor = sheet.Cells[row, 5].Text
                };
                db.xls_AtticRooms.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_RoofingData(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_RoofingData
                {
                    GeneralProjectDataId = projectId,
                    Type = sheet.Cells[row, 1].Text,
                    Length_m = sheet.Cells[row, 2].Text
                };
                db.xls_RoofingData.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_SundryItems(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_SundryItems
                {
                    GeneralProjectDataId = projectId,
                    Label = sheet.Cells[row, 1].Text,
                    Material_name = sheet.Cells[row, 2].Text,
                    Quote_Size = sheet.Cells[row, 3].Text,
                    Depth_mm = sheet.Cells[row, 4].Text,
                    Length_mm = sheet.Cells[row, 5].Text,
                    Type = sheet.Cells[row, 6].Text,
                    Description = sheet.Cells[row, 7].Text,
                    Final_Price_R = sheet.Cells[row, 8].Text
                };
                db.xls_SundryItems.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Fasteners(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Fasteners
                {
                    GeneralProjectDataId = projectId,
                    Ref = sheet.Cells[row, 1].Text,
                    Object = sheet.Cells[row, 2].Text,
                    Quantity = sheet.Cells[row, 3].Text,
                    Organ_type = sheet.Cells[row, 4].Text,
                    Material_name = sheet.Cells[row, 5].Text,
                    Manufacturer = sheet.Cells[row, 6].Text,
                    Usage = sheet.Cells[row, 7].Text,
                    Type = sheet.Cells[row, 8].Text,
                    Diameter_mm = sheet.Cells[row, 9].Text,
                    Length_mm = sheet.Cells[row, 10].Text,
                    Package = sheet.Cells[row, 11].Text,
                    Price_Package_R = sheet.Cells[row, 12].Text
                };
                db.xls_Fasteners.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_DoorsandWindows(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_DoorsandWindows
                {
                    GeneralProjectDataId = projectId,
                    Frame_label = sheet.Cells[row, 1].Text,
                    Type = sheet.Cells[row, 2].Text,
                    Width_mm = sheet.Cells[row, 3].Text,
                    Height_mm = sheet.Cells[row, 4].Text,
                    Name = sheet.Cells[row, 5].Text,
                    Description = sheet.Cells[row, 6].Text,
                    Quantity = sheet.Cells[row, 7].Text
                };
                db.xls_DoorsandWindows.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Decking(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Decking
                {
                    GeneralProjectDataId = projectId,
                    Quantity = sheet.Cells[row, 1].Text,
                    Label = sheet.Cells[row, 2].Text,
                    Material = sheet.Cells[row, 3].Text,
                    Product_code = sheet.Cells[row, 4].Text,
                    Thickness_mm = sheet.Cells[row, 5].Text,
                    Width_mm = sheet.Cells[row, 6].Text,
                    Length_mm = sheet.Cells[row, 7].Text,
                    Cut_Width_mm = sheet.Cells[row, 8].Text,
                    Cut_Length_mm = sheet.Cells[row, 9].Text,
                    Weight_kg = sheet.Cells[row, 10].Text,
                    Price_R = sheet.Cells[row, 11].Text
                };
                db.xls_Decking.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_Sheeting(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_Sheeting
                {
                    GeneralProjectDataId = projectId,
                    Quantity = sheet.Cells[row, 1].Text,
                    Label = sheet.Cells[row, 2].Text,
                    Sum = sheet.Cells[row, 3].Text,
                    Material = sheet.Cells[row, 4].Text,
                    Product_code = sheet.Cells[row, 5].Text,
                    Thickness_mm = sheet.Cells[row, 6].Text,
                    Width_mm = sheet.Cells[row, 7].Text,
                    Length_mm = sheet.Cells[row, 8].Text,
                    Cut_Width_mm = sheet.Cells[row, 9].Text,
                    Cut_Length_mm = sheet.Cells[row, 10].Text,
                    Weight_kg = sheet.Cells[row, 11].Text,
                    Price_R = sheet.Cells[row, 12].Text
                };
                db.xls_Sheeting.Add(record);
            }
            db.SaveChanges();
        }
        public void Importxls_CO2material(ExcelWorksheet sheet, int projectId)
        {
            int rowCount = sheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                if (IsRowEmpty(sheet, row, 1, 2)) // Check "Name" (1) and "Label" (2)
                {
                    break; // stops loop
                }

                var record = new xls_CO2material
                {
                    GeneralProjectDataId = projectId,
                    MaterialID = sheet.Cells[row, 1].Text,
                    Material = sheet.Cells[row, 2].Text,
                    Sub_category = sheet.Cells[row, 3].Text,
                    Site_fixed = sheet.Cells[row, 4].Text,
                    kg = sheet.Cells[row, 5].Text,
                    m3 = sheet.Cells[row, 6].Text,
                    m2 = sheet.Cells[row, 7].Text,
                    m = sheet.Cells[row, 8].Text,
                    Result = sheet.Cells[row, 9].Text,
                    Unit = sheet.Cells[row, 10].Text
                };
                db.xls_CO2material.Add(record);
            }
            db.SaveChanges();
        }

    }
}
