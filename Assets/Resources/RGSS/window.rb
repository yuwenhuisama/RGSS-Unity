require 'type_check_util'
require 'bitmap'
require 'rect'
require 'viewport'

class Window
  include TypeCheckUtil

  attr_reader :handler

  def initialize(x = 0, y = 0, width = 0, height = 0)
    check_arguments([x, y, width, height], [Integer, Integer, Integer, Integer])
    @handler = Unity::Window.new_xywh(x, y, width, height, Viewport::DEFAULT_VIEWPORT.handler)
    self.contents = Window::DEFAULT_CONTENTS_BITMAP
    self.padding = 24
    self.padding_bottom = 24
  end

  def move(x, y, w, h)
    check_arguments([x, y, w, h], [Integer, Integer, Integer, Integer])
    @handler.move(x, y, w, h)
  end

  def windowskin
    Bitmap.new @handler.windowskin
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
    if @handler.viewport == Viewport::DEFAULT_VIEWPORT.handler
      nil
    else
      Viewport.new @handler.viewport
    end
  end

  def viewport=(viewport)
    check_arguments([viewport], [Viewport])
    @handler.viewport = viewport.handler
  end

  def eql?(other)
    if self == other
      true
    end
    self.handler == other.handler
  end

  def hash
    @handler.hash
  end

  def opacity=(opacity)
    check_type(opacity, Integer)
    self.handler.opacity = opacity.clamp(0, 255)
  end

  def opacity
    self.handler.opacity
  end

  def back_opacity=(back_opacity)
    check_type(back_opacity, Integer)
    self.handler.back_opacity = back_opacity.clamp(0, 255)
  end

  def back_opacity
    self.handler.back_opacity
  end

  def contents_opacity=(contents_opacity)
    check_type(contents_opacity, Integer)
    self.handler.contents_opacity = contents_opacity.clamp(0, 255)
  end

  def contents_opacity
    self.handler.contents_opacity
  end

  def openness=(openness)
    check_type(openness, Integer)
    self.handler.openness = openness.clamp(0, 255)
  end

  def openness
    self.handler.openness
  end

  def width=(width)
    check_type(width, Integer)
    if width < 0
      raise ArgumentError.new("width must be positive")
    end
    self.handler.width = width
  end

  def width
    self.handler.width
  end

  def height=(height)
    check_type(height, Integer)
    if height < 0
      raise ArgumentError.new("height must be positive")
    end
    self.handler.height = height
  end
  
  def height
    self.handler.height
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
    :z => Integer,
    :ox => Integer,
    :oy => Integer,
    :padding => Integer,
    :padding_bottom => Integer,
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

  DEFAULT_CONTENTS_BITMAP = Bitmap.new(1, 1)
end