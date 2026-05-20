export interface DnsRule {
  pattern: string
  type: number
  mode: string
  value: string
  isEnabled: boolean
  priority: number
  ipBase: string
}