﻿import React, { ReactNode, useState, useEffect } from 'react'
import { NavLinkProps } from 'react-router-dom'
import SelectSearch from 'components/Input/SelectSearch'

import { User, IdentityClaim } from 'utils/authentication'
import { Container, Nav, Link as NavLink } from './styles'

import { OrganizationView, OrganizationYearView, Quarter } from 'models/OrganizationYear'


interface NavigationProps {
  user: User
  orgYearOptions: any
  orgYearChange: (organizationGuid: string, yearGuid: string) => void
  children: ReactNode
}

const RenderLinks = (props: {user: User, children: ReactNode}): JSX.Element => {
  return (
    <>
      <ul className='d-flex' data-testid='nav-list'>
        {React.Children.toArray(props.children).map((child, index) => {
          return (
            <li key={`nav-item-${index}`}>
              {child}
            </li>
          )
        })}
      </ul>
      <div>Authenticated as {`${props.user.firstName} ${props.user.lastName}`}</div>
    </>
  )
}

const OrganizationYearSelect = ({user, onChange}: {user: User, onChange}) => {
  const [organizations, setOrganizations] = useState<OrganizationView[]>([])
  const [years, setYears] = useState<OrganizationYearView[]>([])

  //handle organization and year selection
  const orgOptions = organizations.map(org => (
    {
      name: org.name,
      value: org.guid
    }
  ))

  const yearOptions = years.map(oy => (
    {
      name: `${oy.year.schoolYear} - ${Quarter[oy.year.quarter]}`,
      value: oy.guid
    }
  ))

  const orgDropdownDisabled: boolean = (!orgOptions || orgOptions.length === 0 || orgOptions.length === 1)
  const currentOrganization: any = orgOptions.find(org => org.value == user.organization.guid)
  const currentYear: any = yearOptions.find(year => year.value == user.organizationYear.guid)

  const handleOrganizationChange = (guid: string): void => {
    const organization: OrganizationView | undefined = organizations.find(org => org.guid === guid)
    user.setOrganization(organization)

    onChange()
  }

  const handleYearChange = (guid: string): void => {
  }

  useEffect(() => {
    user
      .getAuthorizedOrganizationsAsync()
      .then(res => {
        setOrganizations(res)  

        user
          .getOrganizationYearsAsync(user.organization.guid)
          .then(res => setYears(res))
      })
  }, [])

  return (
    <div className='position-absolute text-white' style={{zIndex: 500, right: '2rem', top: 0}}>
      {
        currentOrganization && currentYear
        ? 
          <div className='d-flex mb-3 mt-2'>
            <div>
              <label htmlFor='organization' className='small'>Organization</label>
              <SelectSearch 
                id='organization'
                disabled={orgDropdownDisabled}
                options={orgOptions} 
                value={user.organization.guid} 
                handleChange={value => handleOrganizationChange(value)}
              />
            </div>
            <div style={{width: '1rem'}} />
            <div>
              <label htmlFor='year' className='small'>Term for '{currentOrganization.name}'</label>
              <SelectSearch 
                id='year'
                options={yearOptions} 
                value={user.organizationYear.guid} 
                handleChange={value => handleYearChange(value)}
              />
            </div>
          </div>
        : null
      }
      </div>
  )
}

export const Navigation = (props: NavigationProps): JSX.Element => {

  //handle the display of navigation links
  const filteredLinks = React.Children.toArray(props.children).filter((child) => {
    if (React.isValidElement(child) && typeof child.type !== 'string') {
      if (child.props.requiredType !== undefined && !props.user.isAuthorized(child.props.requiredType)) {
        return false
      }
      return true
    }
  })

  return (
    <Container className='d-flex justify-content-between position-fixed' style={{zIndex: 1030}}>
      <Nav linkCount={filteredLinks.length}>
        <RenderLinks user={props.user} children={filteredLinks} />
      </Nav>
      <OrganizationYearSelect user={props.user} onChange={props.orgYearChange} />
    </Container>
  )
}


export const Link = (props: NavLinkProps): JSX.Element => (
  <NavLink
    requiredtype={IdentityClaim.Coordinator}
    {...props}
  >
    {props.children}
  </NavLink>
)

interface ProtectedLinkProps extends NavLinkProps {
  requiredType?: IdentityClaim
}

export const ProtectedLink = ({ to, requiredType, ...props }: ProtectedLinkProps): JSX.Element => (
  <NavLink
    to={to}
    requiredtype={requiredType}
    {...props}
  >
    {props.children}
  </NavLink>
)
