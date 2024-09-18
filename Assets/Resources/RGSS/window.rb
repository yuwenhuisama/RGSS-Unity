require 'type_check_util'
require 'bitmap'
require 'rect'
require 'viewport'

class Window
  include TypeCheckUtil

  attr_reader :handler

  def intialize(x = 0, y = 0, width = 0, height = 0)
    check_arguments([x, y, width, height], [Integer, Integer, Integer, Integer])
    @handler = Unity::Window.new(x, y, width, height)
  end

  def move(x, y, w, h)
    check_arguments([x, y, w, h], [Integer, Integer, Integer, Integer])
    @handler.move(x, y, w, h)
  end

  def windowskin
    Bitmap.new(@handler.windowskin)
  end

  def windowskin=(bitmap)
    check_arguments([bitmap], [Bitmap])
    @handler.windowskin = bitmap.handler
  end

  def contents
    Bitmap.new(@handler.contents)
  end

  def contents=(contents)
    check_arguments([contents], [Bitmap])
    @handler.contents = contents.handler
  end

  def cursor_rect
    Rect.new(@handler.cursor_rect)
  end

  def cursor_rect=(rect)
    check_arguments([rect], [Rect])
    @handler.cursor_rect = rect.handler
  end

  def viewport
    Viewport.new(@handler.viewport)
  end

  def viewport=(viewport)
    check_arguments([viewport], [Viewport])
    @handler.viewport = viewport.handler
  end

  [:dispose, :disposed?, :update, :open?, :close?].each do |method|
    define_method(method) do |*args|
      @handler.send(method, *args)
    end
  end

  TYPE_CHECK_MAP = {
    :active => [TrueClass, FalseClass],
    :visible => [TrueClass, FalseClass],
    :arrows_visible => [TrueClass, FalseClass],
    :pause => [TrueClass, FalseClass],
    :x => Integer,
    :y => Integer,
    :width => Integer,
    :height => Integer,
    :z => Integer,
    :ox => Integer,
    :oy => Integer,
    :padding => Integer,
    :padding_bottom => Integer,
    :opacity => Integer,
    :back_opacity => Integer,
    :contents_opacity => Integer,
    :openness => Integer,
  }

  TYPE_CHECK_MAP.each do |prop, type|
    define_method(prop) do
      @handler.send(prop)
    end
    define_method("#{prop}=") do |value|
      check_type(value, type)
      @handler.send("#{prop}=", value)
    end
  end
end