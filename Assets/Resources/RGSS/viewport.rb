require 'type_check_util'
require 'color'
require 'tone'
require 'rect'
require 'graphics'

class Viewport
  include TypeCheckUtil

  attr_reader :handler

  def initialize(*args)
    if args.size == 0
      @handler = Unity::Viewport.new_without_rect
    elsif args.size == 1
      check_arguments(args, [Rect])
      rect, = args
      @handler = Unity::Viewport.new_xyrw(rect.x, rect.y, rect.w. rect.h)
    elsif args.size == 4
      check_arguments(args, [Integer, Integer, Integer, Integer])
      @handler = Unity::Viewport.new_xyrw(args[0], args[1], args[2], args[3])
    else
      raise ArgumentError.new("Invalid number of arguments")
    end
  end

  def flash(color, duration)
    check_arguments([color, duration], [Color, [Integer, NilClass]])
    if color.nil?
      @handler.flash(nil, duration)
    else
      @handler.flash(color.handler, duration)
    end
  end

  [:dispose, :disposed?, :update].each do |method|
    define_method(method) do |*args|
      @handler.send(method, *args)
    end
  end

  def color
    Color.new(@handler.color)
  end

  def color=(value)
    check_arguments([value], [Color])
    @handler.color = value.handler
  end

  def tone
    Tone.new(@handler.tone)
  end

  def tone=(tone)
    check_arguments([tone], [Tone])
    @handler.tone = tone.handler
  end

  def rect
    Rect.new(@handler.rect)
  end

  def rect=(rect)
    check_arguments([rect], [Rect])
    @handler.rect = rect.handler
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

  [:z, :ox, :oy].each do |prop|
    define_method(prop) { @handler.send(prop) }
    define_method("#{prop}=") do |value|
      check_type(value, Integer)
      @handler.send("#{prop}=", value)
    end
  end

  DEFAULT_VIEWPORT = Viewport.new(0, 0, Graphics.width, Graphics.height)
end