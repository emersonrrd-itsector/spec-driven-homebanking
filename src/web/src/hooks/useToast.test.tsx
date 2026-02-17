import { describe, it, expect, vi } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import type { ReactNode } from 'react'
import { ToastProvider } from '../context/ToastContext'
import { useToast } from './useToast'

/**
 * Tests for useToast hook
 * Tests the convenience methods: success, error, info
 */
describe('useToast Hook', () => {
  it('provides success, error, and info methods', () => {
    const wrapper = ({ children }: { children: ReactNode }) => (
      <ToastProvider>{children}</ToastProvider>
    )

    const { result } = renderHook(() => useToast(), { wrapper })

    expect(result.current).toHaveProperty('success')
    expect(result.current).toHaveProperty('error')
    expect(result.current).toHaveProperty('info')
    expect(typeof result.current.success).toBe('function')
    expect(typeof result.current.error).toBe('function')
    expect(typeof result.current.info).toBe('function')
  })

  it('success method adds success toast', () => {
    const wrapper = ({ children }: { children: ReactNode }) => (
      <ToastProvider>{children}</ToastProvider>
    )

    const { result } = renderHook(() => useToast(), { wrapper })

    let toastId = ''

    act(() => {
      toastId = result.current.success('Success message')
    })

    expect(toastId).toBeDefined()
    expect(typeof toastId).toBe('string')
  })

  it('error method adds error toast', () => {
    const wrapper = ({ children }: { children: ReactNode }) => (
      <ToastProvider>{children}</ToastProvider>
    )

    const { result } = renderHook(() => useToast(), { wrapper })

    let toastId = ''

    act(() => {
      toastId = result.current.error('Error message')
    })

    expect(toastId).toBeDefined()
    expect(typeof toastId).toBe('string')
  })

  it('info method adds info toast', () => {
    const wrapper = ({ children }: { children: ReactNode }) => (
      <ToastProvider>{children}</ToastProvider>
    )

    const { result } = renderHook(() => useToast(), { wrapper })

    let toastId = ''

    act(() => {
      toastId = result.current.info('Info message')
    })

    expect(toastId).toBeDefined()
    expect(typeof toastId).toBe('string')
  })

  it('returns unique toast IDs', () => {
    const wrapper = ({ children }: { children: ReactNode }) => (
      <ToastProvider>{children}</ToastProvider>
    )

    const { result } = renderHook(() => useToast(), { wrapper })

    let id1: string, id2: string, id3: string

    act(() => {
      id1 = result.current.success('Toast 1')
      id2 = result.current.success('Toast 2')
      id3 = result.current.success('Toast 3')
    })

    expect(id1!).not.toBe(id2!)
    expect(id2!).not.toBe(id3!)
    expect(id1!).not.toBe(id3!)
  })

  it('allows custom duration for toast', () => {
    const wrapper = ({ children }: { children: ReactNode }) => (
      <ToastProvider>{children}</ToastProvider>
    )

    const { result } = renderHook(() => useToast(), { wrapper })

    let toastId: string

    act(() => {
      toastId = result.current.success('Custom duration', 3000)
    })

    expect(toastId!).toBeDefined()
  })

  it('throws error when used outside ToastProvider', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

    expect(() => {
      renderHook(() => useToast())
    }).toThrow('useToastContext must be used inside ToastProvider')

    consoleSpy.mockRestore()
  })
})
