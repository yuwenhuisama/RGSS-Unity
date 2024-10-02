# encoding: utf-8
require 'type_check_util'

class Viewport
  include TypeCheckUtil

  attr_reader :__handler__

  def initialize(*args)
    if args.size == 0
      @__handler__ = Unity::Viewport.new_without_rect
    elsif args.size == 1
      check_arguments(args, [Rect])
      rect, = args
      @__handler__ = Unity::Viewport.new_xyrw(rect.x, rect.y, rect.w. rect.h)
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])
      @__handler__ = Unity::Viewport.new_xyrw(args[0], args[1], args[2], args[3])
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def flash(color, duration)
    check_arguments([color, duration], [Color, [Integer, NilClass]])
    if color.nil?
      @__handler__.flash(nil, duration)
    else
      @__handler__.flash(color.__handler__, duration)
    end
  end

  [:dispose, :disposed?, :update].each do |method|
    define_method(method) do |*args|
      @__handler__.send(method, *args)
    end
  end

  def color
    Color.new(@__handler__.color)
  end

  def color=(value)
    check_arguments([value], [Color])
    @__handler__.color = value.__handler__
  end

  def tone
    Tone.new(@__handler__.tone)
  end

  def tone=(tone)
    check_arguments([tone], [Tone])
    @__handler__.tone = tone.__handler__
  end

  def rect
    Rect.new(@__handler__.rect)
  end

  def rect=(rect)
    check_arguments([rect], [Rect])
    @__handler__.rect = rect.__handler__
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

  [:z, :ox, :oy].each do |prop|
    define_method(prop) { @__handler__.send(prop) }
    define_method("#{prop}=") do |value|
      check_type(value, Integer)
      @__handler__.send("#{prop}=", value)
    end
  end

  DEFAULT_VIEWPORT = Viewport.new(0, 0, Graphics.width, Graphics.height)
end