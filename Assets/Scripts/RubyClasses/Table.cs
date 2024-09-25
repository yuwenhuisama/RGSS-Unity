using System;
using MRuby.Library.Mapper;

namespace RGSSUnity.RubyClasses
{
    using MRuby.Library.Language;

    internal class TableData : RubyData
    {
        public long XSize;
        public long YSize;
        public long ZSize;
        public Int16[] Data;

        public TableData(RbState state) : base(state)
        {
        }
    }

    [RbClass("Table", "Object", "Unity")]
    public static class Table
    {
        [RbClassMethod("new_xyz")]
        private static RbValue New(RbState state, RbValue self, RbValue xsize, RbValue ysize, RbValue zsize)
        {
            var xSize = xsize.ToIntUnchecked();
            var ySize = ysize.ToIntUnchecked();
            var zSize = zsize.ToIntUnchecked();

            if (xSize < 0 || ySize < 0 || zSize < 0)
            {
                state.RaiseRGSSError("Invalid size");
                return state.RbNil;
            }

            var tableData = CreateTableData(state, xSize, ySize, zSize);

            var cls = self.ToClass();
            var res = cls.NewObjectWithRData(tableData);
            return res;
        }

        [RbInstanceMethod("resize")]
        private static RbValue Resize(RbState state, RbValue self, RbValue xsize, RbValue ysize, RbValue zsize)
        {
            var xSize = xsize.ToIntUnchecked();
            var ySize = ysize.ToIntUnchecked();
            var zSize = zsize.ToIntUnchecked();

            if (xSize < 0 || ySize < 0 || zSize < 0)
            {
                state.RaiseRGSSError("Invalid size");
                return state.RbNil;
            }

            var tableData = self.GetRDataObject<TableData>();
            var newSize = xSize;
            if (ySize != 0)
            {
                newSize *= ySize;
                if (zSize != 0)
                {
                    newSize *= zSize;
                }
            }

            tableData.XSize = xSize;
            tableData.YSize = ySize;
            tableData.ZSize = zSize;

            Array.Resize(ref tableData.Data, (int)newSize);
            return state.RbNil;
        }

        [RbInstanceMethod("get_x")]
        private static RbValue GetX(RbState state, RbValue self, RbValue x)
        {
            var tableData = self.GetRDataObject<TableData>();
            var unboxedX = x.ToIntUnchecked();
            if (unboxedX > tableData.XSize)
            {
                state.RaiseRGSSError("Index out of bounds");
                return state.RbNil;
            }
            var data = tableData.Data[unboxedX];
            return ((int)data).ToValue(state);
        }

        [RbInstanceMethod("get_xy")]
        private static RbValue GetXY(RbState state, RbValue self, RbValue x, RbValue y)
        {
            var tableData = self.GetRDataObject<TableData>();
            var unboxedX = x.ToIntUnchecked();
            var unboxedY = y.ToIntUnchecked();
            var index = unboxedX + unboxedY * tableData.XSize;
            if (index > tableData.Data.Length)
            {
                state.RaiseRGSSError("Index out of bounds");
                return state.RbNil;
            }
            
            var data = tableData.Data[index];
            return ((int)data).ToValue(state);
        }

        [RbInstanceMethod("get_xyz")]
        private static RbValue GetXYZ(RbState state, RbValue self, RbValue x, RbValue y, RbValue z)
        {
            var tableData = self.GetRDataObject<TableData>();
            var unboxedX = x.ToIntUnchecked();
            var unboxedY = y.ToIntUnchecked();
            var unboxedZ = z.ToIntUnchecked();

            var index = unboxedX + unboxedY * tableData.XSize + unboxedZ * tableData.XSize * tableData.YSize;
            
            if (index > tableData.Data.Length)
            {
                state.RaiseRGSSError("Index out of bounds");
                return state.RbNil;
            }
            
            var data = tableData.Data[index];
            return ((int)data).ToValue(state);
        }
        
        [RbInstanceMethod("set_x")]
        private static RbValue SetX(RbState state, RbValue self, RbValue x, RbValue value)
        {
            var tableData = self.GetRDataObject<TableData>();
            var unboxedX = x.ToIntUnchecked();
            if (unboxedX > tableData.XSize)
            {
                state.RaiseRGSSError("Index out of bounds");
                return state.RbNil;
            }
            var unboxedValue = value.ToIntUnchecked();
            tableData.Data[unboxedX] = (Int16)unboxedValue;
            return state.RbNil;
        }
        
        [RbInstanceMethod("set_xy")]
        private static RbValue SetXY(RbState state, RbValue self, RbValue x, RbValue y, RbValue value)
        {
            var tableData = self.GetRDataObject<TableData>();
            var unboxedX = x.ToIntUnchecked();
            var unboxedY = y.ToIntUnchecked();
            var index = unboxedX + unboxedY * tableData.XSize;
            if (index > tableData.Data.Length)
            {
                state.RaiseRGSSError("Index out of bounds");
                return state.RbNil;
            }
            var unboxedValue = value.ToIntUnchecked();
            tableData.Data[index] = (Int16)unboxedValue;
            return state.RbNil;
        }
        
        [RbInstanceMethod("set_xyz")]
        private static RbValue SetXYZ(RbState state, RbValue self, RbValue x, RbValue y, RbValue z, RbValue value)
        {
            var tableData = self.GetRDataObject<TableData>();
            var unboxedX = x.ToIntUnchecked();
            var unboxedY = y.ToIntUnchecked();
            var unboxedZ = z.ToIntUnchecked();
            var index = unboxedX + unboxedY * tableData.XSize + unboxedZ * tableData.XSize * tableData.YSize;
            if (index > tableData.Data.Length)
            {
                state.RaiseRGSSError("Index out of bounds");
                return state.RbNil;
            }
            var unboxedValue = value.ToIntUnchecked();
            tableData.Data[index] = (Int16)unboxedValue;
            return state.RbNil;
        }

        private static TableData CreateTableData(RbState state, long xSize, long ySize, long zSize)
        {
            var size = xSize;
            if (ySize != 0)
            {
                size *= ySize;
                if (zSize != 0)
                {
                    size *= zSize;
                }
            }

            var tableData = new TableData(state)
            {
                XSize = xSize,
                YSize = ySize,
                ZSize = zSize,
                Data = new Int16[size],
            };
            return tableData;
        }
    }
}