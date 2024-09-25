require 'type_check_util'

class Bitmap
  include TypeCheckUtil

  attr_reader :__handler__

  def initialize(*args)
    if args.size == 1
      if args[0].is_a? Unity::Bitmap
        @__handler__, = args
      elsif args[0].is_a? String
        filename, = args
        @__handler__ = Unity::Bitmap.new_filename(filename)
      else
        raise TypeError, "Invalid argument type"
      end
    elsif args.size == 2
      check_arguments(args, [Integer, Integer])
      width, height = args
      @__handler__ = Unity::Bitmap.new_wh(width, height)
    else
      raise ArgumentError, "Invalid number of arguments"
    end

    self.font = Font::DEFAULT_FONT
  end

  def blt(x, y, src_bitmap, src_rect, opacity = 255)
    check_arguments([x, y, src_bitmap, src_rect, opacity], [Integer, Integer, Bitmap, Rect, Integer])
    @__handler__.blt(x, y, src_bitmap.__handler__, src_rect.__handler__, opacity)
  end

  def stretch_blt(dest_rect, src_bitmap, src_rect, opacity = 255)
    check_arguments([dest_rect, src_bitmap, src_rect, opacity], [Rect, Bitmap, Rect, Integer])
    @__handler__.stretch_blt(dest_rect.__handler__, src_bitmap.__handler__, src_rect.__handler__, opacity)
  end

  def fill_rect(*args)
    if args.size == 2
      check_arugments(args, [Rect, Color])
      rect, color = args

      @__handler__.fill_rect(rect.x, rect.y, rect.width, rect.height, color.__handler__)
    elsif args.size == 5
      check_arguments(args, [Integer, Integer, Integer, Integer, Color])
      x, y, width, height, color = args

      @__handler__.fill_rect(x, y, width, height, color.__handler__)
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

      @__handler__.gradient_fill_rect(rect.x, rect.y, rect.w. rect.h, color1.__handler__, color2.__handler__, vertical)
    elsif args.size == 6 || args.size == 7
      check_arguments(args[0..5], [Integer, Integer, Integer, Integer, Color, Color])
      x, y, width, height, color1, color2, vertical = args

      check_arguments([vertical], [[TrueClass, FalseClass, NilClass]])
      vertical ||= false

      @__handler__.gradient_fill_rect(x, y, width, height, color1.__handler__, color2.__handler__, vertical)
    else
      raise ArgumentError, "Invalid number of arguments"
    end
  end

  [:dispose, :disposed?, :clear, :blur].each do |method|
    define_method(method) do
      @__handler__.send(method)
    end
  end

  def clear_rect(*args)
    if args.size == 1
      check_arguments(args, [Rect])
      rect, = args

      @__handler__.clear_rect(rect.x, rect.y, rect.w, rect.h)
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])
      x, y, width, height = args

      @__handler__.clear_rect(x, y, width, height)
    else
      raise ArgumentError, "Invalid number of arguments"
    end
  end

  def get_pixel(x, y)
    check_arguments([x, y], [Integer, Integer])

    Color.new @__handler__.get_pixel(x, y)
  end

  def set_pixel(x, y, color)
    check_arguments([x, y, color], [Integer, Integer, Color])

    @__handler__.set_pixel(x, y, color.__handler__)
  end

  def radial_blur(angle, division)
    check_arguments([angle, division], [Integer, Integer])
    @__handler__.radial_blur(angle, division)
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
      @__handler__.draw_text(x, y, width, height, str, align)
    elsif args.size == 2 || args.size == 3
      check_arguments(args[0..1], [Rect, Object])
      rect, str, align = args

      unless str.is_a? String
        str = str.to_s
      end

      check_arguments([align], [[Integer, NilClass]])
      align ||= 0

      msgbox "align type: #{align}"
      
      @__handler__.draw_text(rect.x, rect.y, rect.width, rect.height, str, align)
    else
      raise ArgumentError, "Invalid number of arguments"
    end
  end

  def text_size(str)
    check_arguments([str], [String])
    Rect.new @__handler__.text_size(str)
  end

  def hue_change(hue)
    check_arguments([hue], [Integer])
    @__handler__.hue_change(hue)
  end

  def rect
    Rect.new @__handler__.rect
  end

  def rect=(rect)
    check_arguments([rect], [Rect])
    @__handler__.rect = rect.__handler__
  end

  def font
    Font.new @__handler__.font
  end

  def font=(font)
    check_arguments([font], [Font])
    @__handler__.font = font.__handler__
  end

  def eql?(other)
    if self == other
      true
    end
    self.__handler__ == other.__handler__
  end

  def hash
    @__handler__.hash
  end

  [:width, :height].each do |method|
    define_method(method) do
      @__handler__.send(method)
    end
  end
end
