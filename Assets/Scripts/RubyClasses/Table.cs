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
        public static RbValue New(RbState state, RbValue self, RbValue xsize, RbValue ysize, RbValue zsize)
        {
            var xSize = xsize.ToIntUnchecked();
            var ySize = ysize.ToIntUnchecked();
            var zSize = zsize.ToIntUnchecked();

            if (xSize < 0 || ySize < 0 || zSize < 0)
            {
                var errorCls = state.GetExceptionClass("RGSSError");
                var exc = state.GenerateExceptionWithNewStr(errorCls, "Invalid size");
                state.Raise(exc);
                return state.RbNil;
            }

            var tableData = CreateTableData(state, xSize, ySize, zSize);

            var cls = state.GetClass("Table");
            var res = cls.NewObjectWithRData(tableData);
            return res;
        }

        [RbInstanceMethod("resize")]
        public static RbValue Resize(RbState state, RbValue self, RbValue xsize, RbValue ysize, RbValue zsize)
        {
            var xSize = xsize.ToIntUnchecked();
            var ySize = ysize.ToIntUnchecked();
            var zSize = zsize.ToIntUnchecked();

            if (xSize < 0 || ySize < 0 || zSize < 0)
            {
                var errorCls = state.GetExceptionClass("RGSSError");
                var exc = state.GenerateExceptionWithNewStr(errorCls, "Invalid size");
                state.Raise(exc);
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
        public static RbValue GetX(RbState state, RbValue self, RbValue x)
        {
            var tableData = self.GetRDataObject<TableData>();
            var unboxedX = state.UnboxInt(x);
            var data = tableData.Data[unboxedX];
            return ((int)(data)).ToValue(state);
        }

        [RbInstanceMethod("get_xy")]
        static RbValue GetXY(RbState state, RbValue self, RbValue x, RbValue y)
        {
            var tableData = self.GetRDataObject<TableData>();
            var unboxedX = state.UnboxInt(x);
            var unboxedY = state.UnboxInt(y);
            var data = tableData.Data[unboxedX + unboxedY * tableData.XSize];
            return ((int)(data)).ToValue(state);
        }

        [RbInstanceMethod("get_xyz")]
        static RbValue GetXYZ(RbState state, RbValue self, RbValue x, RbValue y, RbValue z)
        {
            var tableData = self.GetRDataObject<TableData>();
            var unboxedX = state.UnboxInt(x);
            var unboxedY = state.UnboxInt(y);
            var unboxedZ = state.UnboxInt(z);
            var data = tableData.Data[unboxedX + unboxedY * tableData.XSize + unboxedZ * tableData.XSize * tableData.YSize];
            return ((int)(data)).ToValue(state);
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