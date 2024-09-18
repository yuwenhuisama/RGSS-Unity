require 'type_check_util'
require 'bitmap'
require 'rect'
require 'viewport'
require 'color'
require 'tone'

class Sprite
  include TypeCheckUtil

  attr_reader :handler

  def initialize(viewport = nil)
    if viewport.nil?
      @handler = Unity::Sprite.new_with_viewport(Viewport::DEFAULT_VIEWPORT.handler)
    else
      @handler = Unity::Sprite.new_with_viewport(viewport.handler)
    end
  end

  def flash(color, duration)
    check_arguments([color, duration], [[Color, NilClass], Integer])
    if color.nil?
      @handler.flash(nil, duration)
    else
      @handler.flash(color.handler, duration)
    end
  end

  [:dispose, :disposed?, :update, :width, :height].each do |method_name|
    define_method(method_name) { @handler.send(method_name) }
  end

  def bitmap
    Bitmap.new @handler.bitmap
  end

  def bitmap=(bitmap)
    check_arguments([bitmap], [[Bitmap, NilClass]])
    @handler.bitmap = bitmap.handler
  end

  def src_rect
    Rect.new @handler.src_rect
  end

  def src_rect=(rect)
    check_arguments([rect], [Rect])
    @handler.src_rect = rect.handler
  end

  def viewport
    if @handler.viewport == Viewport::DEFAULT_VIEWPORT.viewport
      return nil
    end
    Viewport.new @handler.viewport
  end

  def viewport=(viewport)
    check_arguments([viewport], [[Viewport, NilClass]])
    if viewport.nil?
      @handler.viewport = Viewport::DEFAULT_VIEWPORT.handler
    else
      @handler.viewport = viewport.handler
    end
  end

  def color
    Color.new @handler.color
  end

  def color=(color)
    check_arguments([color], [Color])
    @handler.color = color.handler
  end

  def tone
    Tone.new @handler.tone
  end

  def tone=(tone)
    check_arguments([tone], [Tone])
    @handler.tone = tone.handler
  end

  def opacity=(opacity)
    check_arguments([opacity], [Integer])
    @handler.opacity = Math.clamp(opacity, 0, 255)
  end

  def opacity
    @handler.opacity
  end

  def blend_type
    @handler.blend_type
  end

  def blend_type=(blend_type)
    check_arguments([blend_type], [Integer])

    if blend_type < 0 || blend_type > 2
      raise ArgumentError.new("Invalid blend type #{blend_type}")
    end

    @handler.blend_type = blend_type
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
    define_method(prop) { @handler.send(prop) }
    define_method("#{prop}=") { |value| @handler.send("#{prop}=", value) }
  end
end