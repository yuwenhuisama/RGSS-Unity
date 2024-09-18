﻿require 'type_check_util'

module Input
  include TypeCheckUtil

  DOWN = :DOWN
  LEFT = :LEFT
  RIGHT = :RIGHT
  UP = :UP

  A = :A
  B = :B
  C = :C
  X = :X
  Y = :Y
  Z = :Z
  L = :L
  R = :R

  SHIFT = :SHIFT
  CTRL = :CTRL
  ALT = :ALT

  F5 = :F5
  F6 = :F6
  F7 = :F7
  F8 = :F8
  F9 = :F9

  VALID_KEYS = Set.new([DOWN, LEFT, RIGHT, UP, A, B, C, X, Y, Z, L, R, SHIFT, CTRL, ALT, F5, F6, F7, F8, F9])

  def press?(key)
    check_arguments([key], [Symbol])
    Unity::Input.press?(key) if VALID_KEYS.include? key
  end

  def trigger?(key)
    check_arguments([key], [Symbol])
    Unity::Input.trigger?(key) if VALID_KEYS.include? key
  end

  def repeat?(key)
    check_arguments([key], [Symbol])
    Unity::Input.repeat?(key) if VALID_KEYS.include? key
  end

  [:update, :dir4, :dir8].each { |prop| define_singleton_method(prop) { Unity::Input.send(prop) } }
end