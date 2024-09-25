require 'type_check_util'

class Plane
  include TypeCheckUtil

  attr_reader :__handler__

  def initialize(viewport = nil)
    check_type(viewport, [Viewport, NilClass])
    if viewport.nil?
      @__handler__ = Unity::Plane.new_with_viewport(Viewport::DEFAULT_VIEWPORT.__handler__)
    else
      @__handler__ = Unity::Plane.new_with_viewport(@viewport.__handler__)
    end
  end

  [:dispose, :disposed?].each { |method_name| define_method(method_name) { @__handler__.send(method_name) } }

  def bitmap
    Bitmap.new @__handler__.bitmap
  end

  def bitmap=(value)
    check_type(value, Bitmap)
    @__handler__.bitmap = value.__handler__
  end

  def viewport
    if @__handler__.viewport == Viewport::DEFAULT_VIEWPORT.__handler__
      return nil
    end
    Viewport.new @__handler__.viewport
  end

  def viewport=
    check_type(value, Viewport)
    @__handler__.viewport = value.__handler__
  end

  def color
    Color.new @__handler__.color
  end

  def color=(value)
    check_type(value, Color)
    @__handler__.color = value.__handler__
  end

  def tone
    Tone.new @__handler__.tone
  end

  def tone=(value)
    check_type(value, Tone)
    @__handler__.tone = value.__handler__
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
    :z => Integer,
    :ox => Integer,
    :oy => Integer,
    :zoom_x => [Integer, Float],
    :zoom_y => [Integer, Float],
    :opacity => Integer,
    :blend_type => Integer,
  }

  TYPE_CHECK_MAP.each do |prop, type|
    define_method(prop) { @__handler__.send(prop) }
    define_method("#{prop}=") do |value|
      check_type(value, type)
      @__handler__.send("#{prop}=", value)
    end
  end
end
