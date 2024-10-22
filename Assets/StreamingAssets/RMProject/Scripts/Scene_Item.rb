# encoding: utf-8
#encoding:utf-8
#==============================================================================
# ■ Scene_Item
#------------------------------------------------------------------------------
# 　物品画面
#==============================================================================

class Scene_Item < Scene_ItemBase
  #--------------------------------------------------------------------------
  # ● 开始处理
  #--------------------------------------------------------------------------
  def start
    super
    create_help_window
    create_category_window
    create_item_window
  end
  #--------------------------------------------------------------------------
  # ● 生成分类窗口
  #--------------------------------------------------------------------------
  def create_category_window
    @category_window = Window_ItemCategory.new
    @category_window.viewport = @viewport
    @category_window.help_window = @help_window
    @category_window.y = @help_window.height
    @category_window.set_handler(:ok,     method(:on_category_ok))
    @category_window.set_handler(:cancel, method(:return_scene))
  end
  #--------------------------------------------------------------------------
  # ● 生成物品窗口
  #--------------------------------------------------------------------------
  def create_item_window
    wy = @category_window.y + @category_window.height
    wh = Graphics.height - wy
    @item_window = Window_ItemList.new(0, wy, Graphics.width, wh)
    @item_window.viewport = @viewport
    @item_window.help_window = @help_window
    @item_window.set_handler(:ok,     method(:on_item_ok))
    @item_window.set_handler(:cancel, method(:on_item_cancel))
    @category_window.item_window = @item_window
  end
  #--------------------------------------------------------------------------
  # ● 分类“确定”
  #--------------------------------------------------------------------------
  def on_category_ok
    @item_window.activate
    @item_window.select_last
  end
  #--------------------------------------------------------------------------
  # ● 物品“确定”
  #--------------------------------------------------------------------------
  def on_item_ok
    $game_party.last_item.object = item
    determine_item
  end
  #--------------------------------------------------------------------------
  # ● 物品“取消”
  #--------------------------------------------------------------------------
  def on_item_cancel
    @item_window.unselect
    @category_window.activate
  end
  #--------------------------------------------------------------------------
  # ● 播放使用物品声效
  #--------------------------------------------------------------------------
  def play_se_for_item
    Sound.play_use_item
  end
  #--------------------------------------------------------------------------
  # ● 使用物品
  #--------------------------------------------------------------------------
  def use_item
    super
    @item_window.redraw_current_item
  end
end
