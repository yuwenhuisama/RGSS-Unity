require 'type_check_util'

class Sprite
  include TypeCheckUtil

  attr_reader :__handler__

  def initialize(viewport = nil)
    if viewport.nil?
      @__handler__ = Unity::Sprite.new_with_viewport(Viewport::DEFAULT_VIEWPORT.__handler__)
    else
      @__handler__ = Unity::Sprite.new_with_viewport(viewport.__handler__)
    end
  end

  def flash(color, duration)
    check_arguments([color, duration], [[Color, NilClass], Integer])
    if color.nil?
      @__handler__.flash(nil, duration)
    else
      @__handler__.flash(color.__handler__, duration)
    end
  end

  [:dispose, :disposed?, :update, :width, :height].each do |method_name|
    define_method(method_name) { @__handler__.send(method_name) }
  end

  def bitmap
    Bitmap.new @__handler__.bitmap
  end

  def bitmap=(bitmap)
    check_arguments([bitmap], [[Bitmap, NilClass]])
    if bitmap.nil?
      @__handler__.bitmap = nil
      return
    end
    @__handler__.bitmap = bitmap.__handler__
  end

  def src_rect
    Rect.new @__handler__.src_rect
  end

  def src_rect=(rect)
    check_arguments([rect], [Rect])
    @__handler__.src_rect = rect.__handler__
  end

  def viewport
    if @__handler__.viewport == Viewport::DEFAULT_VIEWPORT.__handler__
      return nil
    end
    Viewport.new @__handler__.viewport
  end

  def viewport=(viewport)
    check_arguments([viewport], [[Viewport, NilClass]])
    if viewport.nil?
      @__handler__.viewport = Viewport::DEFAULT_VIEWPORT.__handler__
    else
      @__handler__.viewport = viewport.__handler__
    end
  end

  def color
    Color.new @__handler__.color
  end

  def color=(color)
    check_arguments([color], [Color])
    @__handler__.color = color.__handler__
  end

  def tone
    Tone.new @__handler__.tone
  end

  def tone=(tone)
    check_arguments([tone], [Tone])
    @__handler__.tone = tone.__handler__
  end

  def opacity=(opacity)
    check_arguments([opacity], [Integer])
    @__handler__.opacity = opacity.clamp(0, 255)
  end

  def opacity
    @__handler__.opacity
  end

  def blend_type
    @__handler__.blend_type
  end

  def blend_type=(blend_type)
    check_arguments([blend_type], [Integer])

    if blend_type < 0 || blend_type > 2
      raise ArgumentError.new("Invalid blend type #{blend_type}")
    end

    @__handler__.blend_type = blend_type
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

  TYPE_CHECK_MAP = {
    :visible => [TrueClass, FalseClass],
    :x => Integer,
    :y => Integer,
    :z => Integer,
    :ox => Integer,
    :oy => Integer,
    :zoom_x => [Integer, Float],
    :zoom_y => [Integer, Float],
    :angle => Integer,
    :wave_amp => Integer,
    :wave_length => Integer,
    :wave_speed => Integer,
    :wave_phase => Integer,
    :mirror => [TrueClass, FalseClass],
    :bush_depth => Integer,
    :bush_opacity => Integer,
  }

  [:visible, :x, :y, :z, :ox, :oy, :zoom_x, :zoom_y,
   :angle, :wave_amp, :wave_length,
   :wave_speed, :wave_phase, :mirror,
   :bush_depth, :bush_opacity, :opacity].each do |prop|
    define_method(prop) { @__handler__.send(prop) }
    define_method("#{prop}=") { |value| @__handler__.send("#{prop}=", value) }
  end
end