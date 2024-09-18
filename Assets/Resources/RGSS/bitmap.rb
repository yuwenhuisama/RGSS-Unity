require 'type_check_util'
require 'color'
require 'rect'
require 'font'

class Bitmap
  include TypeCheckUtil

  attr_reader :handler

  def initialize(*args)
    if args.size == 1
      if args[0].is_a? Unity::Bitmap
        @handler, = args
      elsif args[0].is_a? String
        filename, = args
        @handler = Unity::Bitmap.new_filename(filename)
      else
        raise TypeError, "Invalid argument type"
      end
    elsif args.size == 2
      check_arguments(args, [Integer, Integer])
      width, height = args
      @handler = Unity::Bitmap.new_wh(width, height)
    else
      raise ArgumentError, "Invalid number of arguments"
    end

    self.font = Font::DEFAULT_FONT
  end

  def blt(x, y, src_bitmap, src_rect, opacity = 255)
    check_arguments([x, y, src_bitmap, src_rect, opacity], [Integer, Integer, Bitmap, Rect, Integer])
    @handler.blt(x, y, src_bitmap.handler, src_rect.handler, opacity)
  end

  def stretch_blt(dest_rect, src_bitmap, src_rect, opacity = 255)
    check_arguments([dest_rect, src_bitmap, src_rect, opacity], [Rect, Bitmap, Rect, Integer])
    @handler.stretch_blt(dest_rect.handler, src_bitmap.handler, src_rect.handler, opacity)
  end

  def fill_rect(*args)
    if args.size == 2
      check_arugments(args, [Rect, Color])
      rect, color = args

      @handler.fill_rect(rect.x, rect.y, rect.width, rect.height, color.handler)
    elsif args.size == 5
      check_arguments(args, [Integer, Integer, Integer, Integer, Color])
      x, y, width, height, color = args

      @handler.fill_rect(x, y, width, height, color.handler)
    else
      raise ArgumentError, "Invalid number of arguments"
    end
  end

  def gradient_fill_rect(*args)
    if args.size == 3 || args.size == 4
      check_arguments(args[0..2], [Rect, Color, Color])
      rect, color1, color2, vertical = args

      check_arguments([vertical], [[TrueClass, FalseClass, NilClass]])
      vertical ||= false

      @handler.gradient_fill_rect(rect.x, rect.y, rect.w. rect.h, color1.handler, color2.handler, vertical)
    elsif args.size == 6 || args.size == 7
      check_arguments(args[0..5], [Integer, Integer, Integer, Integer, Color, Color])
      x, y, width, height, color1, color2, vertical = args

      check_arguments([vertical], [[TrueClass, FalseClass, NilClass]])
      vertical ||= false

      @handler.gradient_fill_rect(x, y, width, height, color1.handler, color2.handler, vertical)
    else
      raise ArgumentError, "Invalid number of arguments"
    end
  end

  [:dispose, :disposed?, :clear, :blur].each do |method|
    define_method(method) do
      @handler.send(method)
    end
  end

  def clear_rect(*args)
    if args.size == 1
      check_arguments(args, [Rect])
      rect, = args

      @handler.clear_rect(rect.x, rect.y, rect.w, rect.h)
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])
      x, y, width, height = args

      @handler.clear_rect(x, y, width, height)
    else
      raise ArgumentError, "Invalid number of arguments"
    end
  end

  def get_pixel(x, y)
    check_arguments([x, y], [Integer, Integer])

    @handler.get_pixel(x, y)
  end

  def set_pixel(x, y, color)
    check_arguments([x, y, color], [Integer, Integer, Color])

    @handler.set_pixel(x, y, color.handler)
  end

  def radial_blur(angle, division)
    check_arguments([angle, division], [Integer, Integer])
    @handler.radial_blur(angle, division)
  end

  def draw_text(*args)
    if args.size == 5 || args.size == 6
      check_arguments(args[0..4], [Integer, Integer, Integer, Integer, Object])
      x, y, width, height, str, align = args

      unless str.is_a? String
        str = str.to_s
      end

      check_arguments([align], [[Integer, NilClass]])
      align ||= 0
      @handler.draw_text(x, y, width, height, str, align)
    elsif args.size == 2 || args.size == 3
      check_arguments(args[0..1], [Rect, Object])
      rect, str, align = args

      unless str.is_a? String
        str = str.to_s
      end

      check_arguments([align], [[Integer, NilClass]])
      align ||= 0
      @handler.draw_text(rect.x, rect.y, rect.w, rect.h, str, align)
    else
      raise ArgumentError, "Invalid number of arguments"
    end
  end

  def text_size(str)
    check_arguments([str], [String])
    Rect.new @handler.text_size(str)
  end

  def hue_change(hue)
    check_arguments([hue], [Integer])
    @handler.hue_change(hue)
  end

  def rect
    Rect.new @handler.rect
  end

  def rect=(rect)
    check_arguments([rect], [Rect])
    @handler.rect = rect.handler
  end

  def font
    Font.new @handler.font
  end

  def font=(font)
    check_arguments([font], [Font])
    @handler.font = font.handler
  end

  [:width, :height].each do |method|
    define_method(method) do
      @handler.send(method)
    end
  end
end
