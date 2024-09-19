require 'type_check_util'
require 'plane'
require 'viewport'
require 'bitmap'
require 'color'
require 'tone'

class Plane
  include TypeCheckUtil

  attr_reader :handler

  def initialize(viewport = nil)
    check_type(viewport, [Viewport, NilClass])
    if viewport.nil?
      @handler = Unity::Plane.new_with_viewport(Viewport::DEFAULT_VIEWPORT.handler)
    else
      @handler = Unity::Plane.new_with_viewport(@viewport.handler)
    end
  end

  [:dispose, :disposed?].each { |method_name| define_method(method_name) { @handler.send(method_name) } }

  def bitmap
    Bitmap.new @handler.bitmap
  end

  def bitmap=(value)
    check_type(value, Bitmap)
    @handler.bitmap = value.handler
  end

  def viewport
    if @handler.viewport == Viewport::DEFAULT_VIEWPORT.handler
      return nil
    end
    Viewport.new @handler.viewport
  end

  def viewport=
    check_type(value, Viewport)
    @handler.viewport = value.handler
  end

  def color
    Color.new @handler.color
  end

  def color=(value)
    check_type(value, Color)
    @handler.color = value.handler
  end

  def tone
    Tone.new @handler.tone
  end

  def tone=(value)
    check_type(value, Tone)
    @handler.tone = value.handler
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

  TYPE_CHECK_MAP = {
    :z => Integer,
    :ox => Integer,
    :oy => Integer,
    :zoom_x => [Integer, Float],
    :zoom_y => [Integer, Float],
    :opacity => Integer,
    :blend_type => Integer,
  }

  TYPE_CHECK_MAP.each do |prop, type|
    define_method(prop) { @handler.send(prop) }
    define_method("#{prop}=") do |value|
      check_type(value, type)
      @handler.send("#{prop}=", value)
    end
  end
end
