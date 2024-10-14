namespace RGSSUnity.RubyClasses
{
    using MRuby.Library.Language;
    using MRuby.Library.Mapper;

    [RbClass("Tilemap", "Object", "Unity")]
    public static class Tilemap
    {
        [RbClassMethod("new_with_viewport")]
        public static RbValue NewWithViewport(RbState state, RbValue self, RbValue viewport)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("dispose")]
        public static RbValue Dispose(RbState state, RbValue self)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("disposed?")]
        public static RbValue Disposed(RbState state, RbValue self)
        {
            return state.RbFalse;
        }
        
        [RbInstanceMethod("update")]
        public static RbValue Update(RbState state, RbValue self)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("viewport")]
        public static RbValue Viewport(RbState state, RbValue self)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("map_data")]
        public static RbValue MapData(RbState state, RbValue self)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("map_data=")]
        public static RbValue MapDataSet(RbState state, RbValue self, RbValue value)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("flash_data")]
        public static RbValue FlashData(RbState state, RbValue self)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("flash_data=")]
        public static RbValue FlashDataSet(RbState state, RbValue self, RbValue value)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("bitmaps")]
        public static RbValue Bitmaps(RbState state, RbValue self)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("bitmaps=")]
        public static RbValue BitmapsSet(RbState state, RbValue self)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("flags")]
        public static RbValue Flags(RbState state, RbValue self)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("flags=")]
        public static RbValue FlagsSet(RbState state, RbValue self, RbValue value)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("visible")]
        public static RbValue Visible(RbState state, RbValue self)
        {
            return state.RbFalse;
        }
        
        [RbInstanceMethod("visible=")]
        public static RbValue VisibleSet(RbState state, RbValue self, RbValue value)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("ox")]
        public static RbValue Ox(RbState state, RbValue self)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("ox=")]
        public static RbValue OxSet(RbState state, RbValue self, RbValue value)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("oy")]
        public static RbValue Oy(RbState state, RbValue self, RbValue value)
        {
            return state.RbNil;
        }
        
        [RbInstanceMethod("oy=")]
        public static RbValue OySet(RbState state, RbValue self, RbValue value)
        {
            return state.RbNil;
        }
    }
}